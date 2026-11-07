using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Robotszimulator.UI
{
    public static class SotetCimsor
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr ablak, int attributum, ref int ertek, int meret);

        public static void Alkalmaz(Window ablak)
        {
            ablak.SourceInitialized += (kuldo, esemeny) => Beallit(ablak);
        }

        public static void Beallit(Window ablak)
        {
            IntPtr kezelo = new WindowInteropHelper(ablak).Handle;
            if (kezelo == IntPtr.Zero) return;

            int sotet = 1;
            if (DwmSetWindowAttribute(kezelo, 20, ref sotet, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(kezelo, 19, ref sotet, sizeof(int));
            }

            int hatter = 0x2B2521;
            DwmSetWindowAttribute(kezelo, 35, ref hatter, sizeof(int));

            int szoveg = 0xF3EDE6;
            DwmSetWindowAttribute(kezelo, 36, ref szoveg, sizeof(int));
        }
    }
}
