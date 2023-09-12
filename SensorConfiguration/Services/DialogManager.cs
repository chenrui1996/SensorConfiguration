using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SensorConfiguration.Services
{
    public class DialogManager
    {
        private static readonly Dictionary<Type, Window> dialogCache = new Dictionary<Type, Window>();

        private static readonly Dictionary<Type, HandyControl.Controls.Window> hcDialogCache = new Dictionary<Type, HandyControl.Controls.Window>();

        public static T GetDialog<T>() where T : Window, new()
        {
            Type dialogType = typeof(T);

            if (dialogCache.ContainsKey(dialogType))
            {
                return dialogCache[dialogType] as T;
            }
            else
            {
                T dialog = new T();
                dialog.Closed += (sender, e) =>
                {
                    dialogCache.Remove(dialogType);
                };
                dialogCache.Add(dialogType, dialog);
                return dialog;
            }
        }

        public static T GetHcDialog<T>() where T : HandyControl.Controls.Window, new()
        {
            Type dialogType = typeof(T);
            if (hcDialogCache.ContainsKey(dialogType))
            {
                return hcDialogCache[dialogType] as T;
            }
            else
            {
                T dialog = new T();
                dialog.Closed += (sender, e) =>
                {
                    hcDialogCache.Remove(dialogType);
                };
                hcDialogCache.Add(dialogType, dialog);
                return dialog;
            }
        }

        public static T GetHcDialog<T>(params object?[]? args) where T : HandyControl.Controls.Window, new()
        {
            Type dialogType = typeof(T);
            if (hcDialogCache.ContainsKey(dialogType))
            {
                return hcDialogCache[dialogType] as T;
            }
            else
            {
                T? dialog = Activator.CreateInstance(dialogType, args) as T;
                if (dialog == null)
                {
                    return null;
                }
                dialog.Closed += (sender, e) =>
                {
                    hcDialogCache.Remove(dialogType);
                };
                hcDialogCache.Add(dialogType, dialog);
                return dialog;
            }
        }
    }
}
