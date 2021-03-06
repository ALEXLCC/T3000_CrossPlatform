﻿namespace T3000.Utilities
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Globalization;
    using System.Threading;
    using System.ComponentModel;

    public static class RuntimeLocalizer
    {
        public static void ChangeCulture(Form form, string cultureCode)
        {
            var culture = CultureInfo.GetCultureInfo(cultureCode);

            Thread.CurrentThread.CurrentUICulture = culture;

            var resources = new ComponentResourceManager(form.GetType());

            ApplyResourceToControl(resources, form, culture);
            resources.ApplyResources(form, "$this", culture);

            //Show font dialog for Unix + Chinese language
            if (Environment.OSVersion.Platform != PlatformID.Win32NT &&
                cultureCode.StartsWith("zh"))
            {
                var font = SystemFonts.GetFontByName("AR PL UMing CN");
                if (font == null)
                {
                    var dialog = new FontDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        form.Font = dialog.Font;
                    }
                }
                else
                {
                    form.Font = font;
                }
                foreach (Control control in form.Controls)
                {
                    control.Font = form.Font;
                }
            }
        }

        private static void ApplyResourceToControl(ComponentResourceManager manager, Control control, CultureInfo info)
        {
            var type = control.GetType();
            // See if this is a MenuStrip
            if (type == typeof(MenuStrip))
            {
                var strip = (MenuStrip)control;
                ApplyResourceToToolStripItemCollection(strip.Items, manager, info);
            }

            // See if this is a ToolStrip
            if (type == typeof(ToolStrip))
            {
                var strip = (ToolStrip)control;
                ApplyResourceToToolStripItemCollection(strip.Items, manager, info);
            }

            // See if this is a DataGridViewColumn
            if (type == typeof(DataGridView))
            {
                var view = (DataGridView)control;
                foreach (DataGridViewColumn item in view.Columns)
                {
                    manager.ApplyResources(item, item.Name, info);
                }
            }

            // Apply to all sub-controls
            foreach (Control subControl in control.Controls)
            {
                ApplyResourceToControl(manager, subControl, info);
                manager.ApplyResources(subControl, subControl.Name, info);
            }

            // Apply to self
            var enabled = control.Enabled;
            manager.ApplyResources(control, control.Name, info);
            control.Enabled = enabled;
        }

        private static void ApplyResourceToToolStripItemCollection(ToolStripItemCollection collection, ComponentResourceManager manager, CultureInfo info)
        { 
            // Apply to all sub items
            foreach (ToolStripItem item in collection)
            {
                var type = item.GetType();
                if (type == typeof(ToolStripMenuItem))
                {
                    var menuItem = (ToolStripMenuItem)item;
                    ApplyResourceToToolStripItemCollection(menuItem.DropDownItems, manager, info);
                }

                var enabled = item.Enabled;
                manager.ApplyResources(item, item.Name, info);
                item.Enabled = enabled;
            }
        }
    }
}
