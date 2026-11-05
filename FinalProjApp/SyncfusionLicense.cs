namespace FinalProjApp
{
    /// <summary>
    /// Registers the Syncfusion license on the phone. The key lives in SyncfusionLicense.local.cs,
    /// which git ignores; copy SyncfusionLicense.local.cs.example to create it. Without that file
    /// the app still builds, and Syncfusion shows its unlicensed banner.
    /// </summary>
    internal static partial class SyncfusionLicense
    {
        public static void Register()
        {
            string? key = null;
            LoadKey(ref key);

            if (!string.IsNullOrEmpty(key))
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
        }

        static partial void LoadKey(ref string? key);
    }
}
