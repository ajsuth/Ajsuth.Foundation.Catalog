using Sitecore.Data.Managers;
using Sitecore.Pipelines;
using Sitecore.Web;
using System;
using System.Linq;
using System.Web;
using Context = Sitecore.Context;
using ItemTypes = Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes;
using DataTemplates = Sitecore.Commerce.XA.Foundation.Common.Constants.DataTemplates;

namespace Ajsuth.Foundation.Catalog.Website.Pipelines
{
    public class CatalogPageItemResolver : Sitecore.Commerce.XA.Foundation.Catalog.Pipelines.CatalogPageItemResolver
    {
        public CatalogPageItemResolver()
            : base()
        {
        }

        public override void Process(PipelineArgs args)
        {
            if (Context.Item == null || SiteContext.CurrentCatalogItem != null)
            {
                return;
            }

            var contextItemType = GetContextItemType();
            switch (contextItemType)
            {
                case ItemTypes.Category:
                case ItemTypes.Product:
                    var isProduct = contextItemType == ItemTypes.Product;
                    var catalogItemIdFromUrl = GetCatalogItemIdFromUrl(isProduct);
                    if (string.IsNullOrEmpty(catalogItemIdFromUrl))
                    {
                        break;
                    }
                    var catalog = StorefrontContext.CurrentStorefront.Catalog;
                    var catalogItem = ResolveCatalogItem(catalogItemIdFromUrl, catalog, isProduct);
                    if (catalogItem == null && !isProduct)
                    {
                        catalogItemIdFromUrl = GetCatalogItemIdFromUrl(true);
                        if (string.IsNullOrEmpty(catalogItemIdFromUrl))
                        {
                            break;
                        }
                        catalogItem = ResolveCatalogItem(catalogItemIdFromUrl, catalog, isProduct);
                    }
                    if (catalogItem == null)
                    {
                        WebUtil.Redirect("~/");
                    }

                    SiteContext.CurrentCatalogItem = catalogItem;
                    break;
            }
        }

        private ItemTypes GetContextItemType()
        {
            var template = TemplateManager.GetTemplate(Context.Item);
            var itemTypes = ItemTypes.Unknown;
            if (template.InheritsFrom(DataTemplates.CategoryPage.ID))
            {
                itemTypes = ItemTypes.Category;
            }
            else if (template.InheritsFrom(DataTemplates.ProductPage.ID))
            {
                itemTypes = ItemTypes.Product;
            }

            return itemTypes;
        }

        private string GetCatalogItemIdFromUrl(bool isProduct)
        {
            var catalogItemId = string.Empty;
            var rawUrl = HttpContext.Current.Request.RawUrl;
            var urlTokens = rawUrl.Split('/');
            if (urlTokens.Any())
            {
                var item = urlTokens.Last();
                var queryStringPosition = item.IndexOf("?", StringComparison.OrdinalIgnoreCase);
                if (queryStringPosition > 0)
                {
                    item = item.Substring(0, queryStringPosition);
                }

                if (isProduct && urlTokens.Length >= 4)
                {
                    var parentCategoryName = urlTokens[urlTokens.Length - 2];
                    item = $"{parentCategoryName}{item}";
                }
                catalogItemId = CatalogUrlManager.ExtractItemId(item);
            }

            return catalogItemId;
        }
    }
}