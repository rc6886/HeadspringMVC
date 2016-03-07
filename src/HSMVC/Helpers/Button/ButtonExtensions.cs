using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HSMVC.Helpers.Button
{
    public static class ButtonExtensions
    {
        public static MvcHtmlString SaveButton(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(@"<button class=""btn btn-default"" type=""submit"">Save</button>");
        }

        public static MvcHtmlString CancelToConferenceIndexButton(this HtmlHelper htmlHelper)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.Action("Index", "Conference");
            return new MvcHtmlString($@"<a class=""btn btn-default"" href=""{url}"" role=""button"">Cancel</a>");
        }
    }
}