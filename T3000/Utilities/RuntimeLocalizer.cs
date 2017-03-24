﻿namespace T3000.Utilities
{
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
        }

        private static void ApplyResourceToControl(ComponentResourceManager manager, Control control, CultureInfo info)
        {  
            // See if this is a menuStrip
            if (control.GetType() == typeof(MenuStrip))
            {
                var strip = (MenuStrip)control;
                ApplyResourceToToolStripItemCollection(strip.Items, manager, info);
            }

            // See if this is a menuStrip
            if (control.GetType() == typeof(DataGridView))
            {
                var view = (DataGridView)control;
                ApplyResourceToColumns(view.Columns, manager, info);
            }

            // Apply to all sub-controls
            foreach (Control subControl in control.Controls)
            {
                ApplyResourceToControl(manager, subControl, info);
                manager.ApplyResources(subControl, subControl.Name, info);
            }

            // Apply to self
            manager.ApplyResources(control, control.Name, info);
        }

        private static void ApplyResourceToToolStripItemCollection(ToolStripItemCollection collection, ComponentResourceManager manager, CultureInfo info)
        { 
            // Apply to all sub items
            foreach (ToolStripMenuItem item in collection)
            {
                if (item.GetType() == typeof(ToolStripMenuItem))
                {
                    var menuitem = item;
                    ApplyResourceToToolStripItemCollection(menuitem.DropDownItems, manager, info);
                }

                manager.ApplyResources(item, item.Name, info);
            }
        }

        private static void ApplyResourceToColumns(DataGridViewColumnCollection collection, ComponentResourceManager manager, CultureInfo info)
        {
            // Apply to all sub items
            foreach (DataGridViewColumn item in collection)
            {
                //if (item.GetType() == typeof(ToolStripMenuItem))
                //{
                 //   var menuitem = item;
                 //   ApplyResourceToToolStripItemCollection(menuitem.DropDownItems, manager, info);
                //}

                manager.ApplyResources(item, item.Name, info);
            }
        }
    }
}