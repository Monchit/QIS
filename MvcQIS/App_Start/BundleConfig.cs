using System.Web;
using System.Web.Optimization;

namespace MvcQIS
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            //-----------------------------------Style----------------------------------//
            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/site.css",
                        "~/Content/jquery.jqGrid/ui.jqgrid.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/Content/select2").Include(
                        "~/Content/select2.css"));

            //bundles.Add(new StyleBundle("~/Content/dcmegamenu").Include(
            //            "~/Content/dcmegamenu.css",
            //            "~/Content/skins/grey.css"));

            //bundles.Add(new StyleBundle("~/Content/fancybox").Include(
            //            "~/Content/jquery.fancybox.css"));

            //-----------------------------------Script----------------------------------//
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        //"~/Scripts/jquery.unobtrusive*",
                        //"~/Scripts/jquery.validate*",
                        "~/Scripts/jquery.formvalidator.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryjqgrid").Include(
                        "~/Scripts/jquery.jqGrid.src.js",
                        "~/Scripts/i18n/grid.locale-en.js"));

            bundles.Add(new ScriptBundle("~/bundles/mousewheel").Include(
                        "~/Scripts/jquery.mousewheel-3.0.6.pack.js"));

            //bundles.Add(new ScriptBundle("~/bundles/dcmegamenu").Include(
            //            "~/Scripts/jquery.dcmegamenu.1.3.3.js",
            //            "~/Scripts/jquery.hoverIntent.js"));

            bundles.Add(new ScriptBundle("~/bundles/noty").Include(
                        "~/Scripts/noty/jquery.noty.js",
                        "~/Scripts/noty/layouts/top.js",
                        "~/Scripts/noty/layouts/topLeft.js",
                        "~/Scripts/noty/layouts/topRight.js",
                        "~/Scripts/noty/layouts/topCenter.js",
                        "~/Scripts/noty/layouts/center.js",
                        "~/Scripts/noty/layouts/centerLeft.js",
                        "~/Scripts/noty/layouts/centerRight.js",
                        "~/Scripts/noty/layouts/bottom.js",
                        "~/Scripts/noty/layouts/bottomLeft.js",
                        "~/Scripts/noty/layouts/bottomRight.js",
                        "~/Scripts/noty/layouts/bottomCenter.js",
                        "~/Scripts/noty/themes/default.js"));

            bundles.Add(new ScriptBundle("~/bundles/select2").Include(
                        "~/Scripts/select2.js"));
            
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
        }
    }
}