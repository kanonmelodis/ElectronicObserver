using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    /// <summary>
    /// Visual Studio 2012 Light theme.
    /// </summary>
    public class VS2012DarkTheme : ThemeBase
    {
        /// <summary>
        /// Applies the specified theme to the dock panel.
        /// </summary>
        /// <param name="dockPanel">The dock panel.</param>
        public override void Apply(DockPanel dockPanel)
        {
            if (dockPanel == null)
            {
                throw new NullReferenceException("dockPanel");
            }

            Measures.SplitterSize = 6;
			dockPanel.Extender.DockPaneCaptionFactory = new VS2012DarkDockPaneCaptionFactory();
			dockPanel.Extender.AutoHideStripFactory = new VS2012DarkAutoHideStripFactory();
			dockPanel.Extender.AutoHideWindowFactory = new VS2012DarkAutoHideWindowFactory();
			dockPanel.Extender.DockPaneStripFactory = new VS2012DarkDockPaneStripFactory();
			dockPanel.Extender.DockPaneSplitterControlFactory = new VS2012DarkDockPaneSplitterControlFactory();
			dockPanel.Extender.DockWindowSplitterControlFactory = new VS2012DarkDockWindowSplitterControlFactory();
			dockPanel.Extender.DockWindowFactory = new VS2012DarkDockWindowFactory();
			dockPanel.Extender.PaneIndicatorFactory = new VS2012DarkPaneIndicatorFactory();
			dockPanel.Extender.PanelIndicatorFactory = new VS2012DarkPanelIndicatorFactory();
			dockPanel.Extender.DockOutlineFactory = new VS2012DarkDockOutlineFactory();
			dockPanel.Skin = CreateVisualStudio2012Dark();
        }

		private class VS2012DarkDockOutlineFactory : DockPanelExtender.IDockOutlineFactory
        {
            public DockOutlineBase CreateDockOutline()
            {
				return new VS2012DarkDockOutline();
            }

            private class VS2012DarkDockOutline : DockOutlineBase
            {
				public VS2012DarkDockOutline()
                {
                    m_dragForm = new DragForm();
                    SetDragForm(Rectangle.Empty);
                    DragForm.BackColor = Color.FromArgb(0xff, 91, 173, 255);
                    DragForm.Opacity = 0.5;
                    DragForm.Show(false);
                }

                DragForm m_dragForm;
                private DragForm DragForm
                {
                    get { return m_dragForm; }
                }

                protected override void OnShow()
                {
                    CalculateRegion();
                }

                protected override void OnClose()
                {
                    DragForm.Close();
                }

                private void CalculateRegion()
                {
                    if (SameAsOldValue)
                        return;

                    if (!FloatWindowBounds.IsEmpty)
                        SetOutline(FloatWindowBounds);
                    else if (DockTo is DockPanel)
                        SetOutline(DockTo as DockPanel, Dock, (ContentIndex != 0));
                    else if (DockTo is DockPane)
                        SetOutline(DockTo as DockPane, Dock, ContentIndex);
                    else
                        SetOutline();
                }

                private void SetOutline()
                {
                    SetDragForm(Rectangle.Empty);
                }

                private void SetOutline(Rectangle floatWindowBounds)
                {
                    SetDragForm(floatWindowBounds);
                }

                private void SetOutline(DockPanel dockPanel, DockStyle dock, bool fullPanelEdge)
                {
                    Rectangle rect = fullPanelEdge ? dockPanel.DockArea : dockPanel.DocumentWindowBounds;
                    rect.Location = dockPanel.PointToScreen(rect.Location);
                    if (dock == DockStyle.Top)
                    {
                        int height = dockPanel.GetDockWindowSize(DockState.DockTop);
                        rect = new Rectangle(rect.X, rect.Y, rect.Width, height);
                    }
                    else if (dock == DockStyle.Bottom)
                    {
                        int height = dockPanel.GetDockWindowSize(DockState.DockBottom);
                        rect = new Rectangle(rect.X, rect.Bottom - height, rect.Width, height);
                    }
                    else if (dock == DockStyle.Left)
                    {
                        int width = dockPanel.GetDockWindowSize(DockState.DockLeft);
                        rect = new Rectangle(rect.X, rect.Y, width, rect.Height);
                    }
                    else if (dock == DockStyle.Right)
                    {
                        int width = dockPanel.GetDockWindowSize(DockState.DockRight);
                        rect = new Rectangle(rect.Right - width, rect.Y, width, rect.Height);
                    }
                    else if (dock == DockStyle.Fill)
                    {
                        rect = dockPanel.DocumentWindowBounds;
                        rect.Location = dockPanel.PointToScreen(rect.Location);
                    }

                    SetDragForm(rect);
                }

                private void SetOutline(DockPane pane, DockStyle dock, int contentIndex)
                {
                    if (dock != DockStyle.Fill)
                    {
                        Rectangle rect = pane.DisplayingRectangle;
                        if (dock == DockStyle.Right)
                            rect.X += rect.Width / 2;
                        if (dock == DockStyle.Bottom)
                            rect.Y += rect.Height / 2;
                        if (dock == DockStyle.Left || dock == DockStyle.Right)
                            rect.Width -= rect.Width / 2;
                        if (dock == DockStyle.Top || dock == DockStyle.Bottom)
                            rect.Height -= rect.Height / 2;
                        rect.Location = pane.PointToScreen(rect.Location);

                        SetDragForm(rect);
                    }
                    else if (contentIndex == -1)
                    {
                        Rectangle rect = pane.DisplayingRectangle;
                        rect.Location = pane.PointToScreen(rect.Location);
                        SetDragForm(rect);
                    }
                    else
                    {
                        using (GraphicsPath path = pane.TabStripControl.GetOutline(contentIndex))
                        {
                            RectangleF rectF = path.GetBounds();
                            Rectangle rect = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
                            using (Matrix matrix = new Matrix(rect, new Point[] { new Point(0, 0), new Point(rect.Width, 0), new Point(0, rect.Height) }))
                            {
                                path.Transform(matrix);
                            }

                            Region region = new Region(path);
                            SetDragForm(rect, region);
                        }
                    }
                }

                private void SetDragForm(Rectangle rect)
                {
                    DragForm.Bounds = rect;
                    if (rect == Rectangle.Empty)
                    {
                        if (DragForm.Region != null)
                        {
                            DragForm.Region.Dispose();
                        }

                        DragForm.Region = new Region(Rectangle.Empty);
                    }
                    else if (DragForm.Region != null)
                    {
                        DragForm.Region.Dispose();
                        DragForm.Region = null;
                    }
                }

                private void SetDragForm(Rectangle rect, Region region)
                {
                    DragForm.Bounds = rect;
                    if (DragForm.Region != null)
                    {
                        DragForm.Region.Dispose();
                    }

                    DragForm.Region = region;
                }
            }
        }

		private class VS2012DarkPanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory
        {
            public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style)
            {
				return new VS2012DarkPanelIndicator( style );
            }

            private class VS2012DarkPanelIndicator : PictureBox, DockPanel.IPanelIndicator
            {
                private static Image _imagePanelLeft = Resources.DockIndicator_PanelLeft_VS2012;
                private static Image _imagePanelRight = Resources.DockIndicator_PanelRight_VS2012;
                private static Image _imagePanelTop = Resources.DockIndicator_PanelTop_VS2012;
                private static Image _imagePanelBottom = Resources.DockIndicator_PanelBottom_VS2012;
                private static Image _imagePanelFill = Resources.DockIndicator_PanelFill_VS2012;
                private static Image _imagePanelLeftActive = Resources.DockIndicator_PanelLeft_VS2012;
                private static Image _imagePanelRightActive = Resources.DockIndicator_PanelRight_VS2012;
                private static Image _imagePanelTopActive = Resources.DockIndicator_PanelTop_VS2012;
                private static Image _imagePanelBottomActive = Resources.DockIndicator_PanelBottom_VS2012;
                private static Image _imagePanelFillActive = Resources.DockIndicator_PanelFill_VS2012;

				public VS2012DarkPanelIndicator( DockStyle dockStyle )
                {
                    m_dockStyle = dockStyle;
                    SizeMode = PictureBoxSizeMode.AutoSize;
                    Image = ImageInactive;
                }

                private DockStyle m_dockStyle;

                private DockStyle DockStyle
                {
                    get { return m_dockStyle; }
                }

                private DockStyle m_status;

                public DockStyle Status
                {
                    get { return m_status; }
                    set
                    {
                        if (value != DockStyle && value != DockStyle.None)
                            throw new InvalidEnumArgumentException();

                        if (m_status == value)
                            return;

                        m_status = value;
                        IsActivated = (m_status != DockStyle.None);
                    }
                }

                private Image ImageInactive
                {
                    get
                    {
                        if (DockStyle == DockStyle.Left)
                            return _imagePanelLeft;
                        else if (DockStyle == DockStyle.Right)
                            return _imagePanelRight;
                        else if (DockStyle == DockStyle.Top)
                            return _imagePanelTop;
                        else if (DockStyle == DockStyle.Bottom)
                            return _imagePanelBottom;
                        else if (DockStyle == DockStyle.Fill)
                            return _imagePanelFill;
                        else
                            return null;
                    }
                }

                private Image ImageActive
                {
                    get
                    {
                        if (DockStyle == DockStyle.Left)
                            return _imagePanelLeftActive;
                        else if (DockStyle == DockStyle.Right)
                            return _imagePanelRightActive;
                        else if (DockStyle == DockStyle.Top)
                            return _imagePanelTopActive;
                        else if (DockStyle == DockStyle.Bottom)
                            return _imagePanelBottomActive;
                        else if (DockStyle == DockStyle.Fill)
                            return _imagePanelFillActive;
                        else
                            return null;
                    }
                }

                private bool m_isActivated = false;

                private bool IsActivated
                {
                    get { return m_isActivated; }
                    set
                    {
                        m_isActivated = value;
                        Image = IsActivated ? ImageActive : ImageInactive;
                    }
                }

                public DockStyle HitTest(Point pt)
                {
                    return this.Visible && ClientRectangle.Contains(PointToClient(pt)) ? DockStyle : DockStyle.None;
                }
            }
        }

		private class VS2012DarkPaneIndicatorFactory : DockPanelExtender.IPaneIndicatorFactory
        {
            public DockPanel.IPaneIndicator CreatePaneIndicator()
            {
				return new VS2012DarkPaneIndicator();
            }

            private class VS2012DarkPaneIndicator : PictureBox, DockPanel.IPaneIndicator
            {
                private static Bitmap _bitmapPaneDiamond = Resources.Dockindicator_PaneDiamond_VS2012;
                private static Bitmap _bitmapPaneDiamondLeft = Resources.Dockindicator_PaneDiamond_Fill_VS2012;
                private static Bitmap _bitmapPaneDiamondRight = Resources.Dockindicator_PaneDiamond_Fill_VS2012;
                private static Bitmap _bitmapPaneDiamondTop = Resources.Dockindicator_PaneDiamond_Fill_VS2012;
                private static Bitmap _bitmapPaneDiamondBottom = Resources.Dockindicator_PaneDiamond_Fill_VS2012;
                private static Bitmap _bitmapPaneDiamondFill = Resources.Dockindicator_PaneDiamond_Fill_VS2012;
                private static Bitmap _bitmapPaneDiamondHotSpot = Resources.Dockindicator_PaneDiamond_Hotspot_VS2012;
                private static Bitmap _bitmapPaneDiamondHotSpotIndex = Resources.DockIndicator_PaneDiamond_HotspotIndex_VS2012;

                private static DockPanel.HotSpotIndex[] _hotSpots = new[]
                    {
                        new DockPanel.HotSpotIndex(1, 0, DockStyle.Top),
                        new DockPanel.HotSpotIndex(0, 1, DockStyle.Left),
                        new DockPanel.HotSpotIndex(1, 1, DockStyle.Fill),
                        new DockPanel.HotSpotIndex(2, 1, DockStyle.Right),
                        new DockPanel.HotSpotIndex(1, 2, DockStyle.Bottom)
                    };

                private GraphicsPath _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

				public VS2012DarkPaneIndicator()
                {
                    SizeMode = PictureBoxSizeMode.AutoSize;
                    Image = _bitmapPaneDiamond;
                    Region = new Region(DisplayingGraphicsPath);
                }

                public GraphicsPath DisplayingGraphicsPath
                {
                    get { return _displayingGraphicsPath; }
                }

                public DockStyle HitTest(Point pt)
                {
                    if (!Visible)
                        return DockStyle.None;

                    pt = PointToClient(pt);
                    if (!ClientRectangle.Contains(pt))
                        return DockStyle.None;

                    for (int i = _hotSpots.GetLowerBound(0); i <= _hotSpots.GetUpperBound(0); i++)
                    {
                        if (_bitmapPaneDiamondHotSpot.GetPixel(pt.X, pt.Y) == _bitmapPaneDiamondHotSpotIndex.GetPixel(_hotSpots[i].X, _hotSpots[i].Y))
                            return _hotSpots[i].DockStyle;
                    }

                    return DockStyle.None;
                }

                private DockStyle m_status = DockStyle.None;

                public DockStyle Status
                {
                    get { return m_status; }
                    set
                    {
                        m_status = value;
                        if (m_status == DockStyle.None)
                            Image = _bitmapPaneDiamond;
                        else if (m_status == DockStyle.Left)
                            Image = _bitmapPaneDiamondLeft;
                        else if (m_status == DockStyle.Right)
                            Image = _bitmapPaneDiamondRight;
                        else if (m_status == DockStyle.Top)
                            Image = _bitmapPaneDiamondTop;
                        else if (m_status == DockStyle.Bottom)
                            Image = _bitmapPaneDiamondBottom;
                        else if (m_status == DockStyle.Fill)
                            Image = _bitmapPaneDiamondFill;
                    }
                }
            }
        }

		private class VS2012DarkAutoHideWindowFactory : DockPanelExtender.IAutoHideWindowFactory
        {
            public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel)
            {
                return new VS2012LightAutoHideWindowControl(panel);
            }
        }

		private class VS2012DarkDockPaneSplitterControlFactory : DockPanelExtender.IDockPaneSplitterControlFactory
        {
            public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane)
            {
				return new VS2012DarkSplitterControl(pane);
            }
        }

		private class VS2012DarkDockWindowSplitterControlFactory : DockPanelExtender.IDockWindowSplitterControlFactory
        {
            public SplitterBase CreateSplitterControl()
            {
				return new VS2012LightDockWindow.VS2012DarkDockWindowSplitterControl();
            }
        }

		private class VS2012DarkDockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory
        {
            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2012DarkDockPaneStrip(pane);
            }
        }

		private class VS2012DarkAutoHideStripFactory : DockPanelExtender.IAutoHideStripFactory
        {
            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
				var strip = new VS2012LightAutoHideStrip(panel);
				strip.BackColor = Color.FromArgb(255, 45, 45, 48);
				strip.BackgroundBrush = new SolidBrush(strip.BackColor);
                return strip;
            }
        }

		private class VS2012DarkDockPaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory
        {
            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2012LightDockPaneCaption(pane);
            }
        }

		private class VS2012DarkDockWindowFactory : DockPanelExtender.IDockWindowFactory
        {
            public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState)
            {
                return new VS2012LightDockWindow(dockPanel, dockState);
            }
        }

		public static DockPanelSkin CreateVisualStudio2012Dark()
        {
			var back = Color.FromArgb(255, 45, 45, 48);
            var specialBlue = Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC);
			var darkdot = Color.FromArgb(255, 70, 70, 74);
			var bluedot = Color.FromArgb(255, 89, 168, 222);
			var activeTab = specialBlue;
            var mouseHoverTab = Color.FromArgb(0xFF, 28, 151, 234);
			var toolActiveTab = Color.FromArgb(255, 30, 30, 30);
			var inactiveTab = back;
            var lostFocusTab = Color.FromArgb(0xFF, 63, 63, 70);
			var inactiveText = Color.FromArgb(255, 241, 241, 241);
			var darkText = Color.FromArgb(255, 208, 208, 208);
            var skin = new DockPanelSkin();

            skin.AutoHideStripSkin.DockStripGradient.StartColor = specialBlue;
			skin.AutoHideStripSkin.DockStripGradient.EndColor = lostFocusTab;
			skin.AutoHideStripSkin.TabGradient.TextColor = darkText;

			skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.StartColor = back;
			skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.EndColor = back;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor = activeTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor = lostFocusTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor = Color.White;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor = inactiveTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor = mouseHoverTab;
			skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor = inactiveText;

            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.StartColor = back;
            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.EndColor = back;

			skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor = toolActiveTab;
			skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor = toolActiveTab;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor = specialBlue;

			skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.StartColor = back;
			skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.EndColor = back;
			skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor = darkText;

			skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.StartColor = specialBlue;
			skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.EndColor = bluedot;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor = Color.White;

			skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.StartColor = inactiveTab;
			skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.EndColor = darkdot;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
			skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor = inactiveText;

            return skin;
        }
    }
}