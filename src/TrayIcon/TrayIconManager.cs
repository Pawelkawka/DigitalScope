using System.Drawing;
using System.Windows.Forms;
using DigitalScope.Core;

namespace DigitalScope.TrayIcon;

public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly Icon       _appIcon;

    public event Action? ShowRequested;
    public event Action? ExitRequested;

    public TrayIconManager()
    {
        _appIcon = LoadAppIcon();

        var menu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            ShowCheckMargin = false,
            BackColor       = Color.FromArgb(32, 32, 32),
            ForeColor       = Color.FromArgb(238, 238, 238),
            Padding         = new Padding(6),
            Renderer        = new DarkMenuRenderer(),
        };

        var showItem = new ToolStripMenuItem("Show");
        showItem.Font      = new Font(showItem.Font, FontStyle.Bold);
        showItem.ForeColor = menu.ForeColor;
        showItem.Click    += (_, _) => ShowRequested?.Invoke();
        menu.Items.Add(showItem);
        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.ForeColor = menu.ForeColor;
        exitItem.Click    += (_, _) => ExitRequested?.Invoke();
        menu.Items.Add(exitItem);

        _icon = new NotifyIcon
        {
            Text             = AppSettings.AppName,
            Icon             = _appIcon,
            ContextMenuStrip = menu,
            Visible          = true,
        };

        _icon.DoubleClick += (_, _) => ShowRequested?.Invoke();
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _appIcon.Dispose();
    }

    private static Icon LoadAppIcon()
    {
        try
        {
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath))
            {
                using var extracted = Icon.ExtractAssociatedIcon(processPath);
                if (extracted is not null)
                    return (Icon)extracted.Clone();
            }
        }
        catch { }

        return (Icon)SystemIcons.Application.Clone();
    }

    private sealed class DarkMenuRenderer() : ToolStripProfessionalRenderer(new DarkMenuColorTable())
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var bounds = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
            Color fill = e.Item.Selected
                ? Color.FromArgb(58, 58, 58)
                : Color.FromArgb(32, 32, 32);

            using var brush = new SolidBrush(fill);
            e.Graphics.FillRectangle(brush, bounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(64, 64, 64));
            var bounds = new Rectangle(Point.Empty, e.ToolStrip.Size);
            bounds.Width  -= 1;
            bounds.Height -= 1;
            e.Graphics.DrawRectangle(pen, bounds);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var y = e.Item.ContentRectangle.Height / 2;
            using var pen = new Pen(Color.FromArgb(64, 64, 64));
            e.Graphics.DrawLine(pen, 10, y, e.Item.Width - 10, y);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.FromArgb(238, 238, 238);
            base.OnRenderItemText(e);
        }
    }

    private sealed class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.FromArgb(32, 32, 32);
        public override Color ImageMarginGradientBegin    => Color.FromArgb(32, 32, 32);
        public override Color ImageMarginGradientMiddle   => Color.FromArgb(32, 32, 32);
        public override Color ImageMarginGradientEnd      => Color.FromArgb(32, 32, 32);
        public override Color MenuBorder                  => Color.FromArgb(64, 64, 64);
        public override Color MenuItemBorder                => Color.FromArgb(70, 70, 70);
        public override Color MenuItemSelected              => Color.FromArgb(58, 58, 58);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(58, 58, 58);
        public override Color MenuItemSelectedGradientEnd   => Color.FromArgb(58, 58, 58);
        public override Color MenuItemPressedGradientBegin  => Color.FromArgb(66, 66, 66);
        public override Color MenuItemPressedGradientMiddle => Color.FromArgb(66, 66, 66);
        public override Color MenuItemPressedGradientEnd    => Color.FromArgb(66, 66, 66);
        public override Color SeparatorDark                 => Color.FromArgb(64, 64, 64);
        public override Color SeparatorLight                => Color.FromArgb(64, 64, 64);
    }
}
