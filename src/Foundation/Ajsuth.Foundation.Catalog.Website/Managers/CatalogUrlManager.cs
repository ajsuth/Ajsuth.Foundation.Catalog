using Sitecore.Commerce.XA.Foundation.Catalog;
using Sitecore.Commerce.XA.Foundation.Common.Context;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using Context = Sitecore.Context;

namespace Ajsuth.Foundation.Catalog.Website.Managers
{
    public class CatalogUrlManager : Sitecore.Commerce.XA.Foundation.CommerceEngine.Managers.CatalogUrlManager
    {
        private static string[] _invalidPathCharacters = new string[11] { "<", ">", "*", "%", "&", ":", "\\", "?", ".", "\"", " " };

        public CatalogUrlManager(IStorefrontContext storefrontContext, ISiteContext siteContext)
            : base(storefrontContext, siteContext)
        {
        }

        public override string BuildCategoryLink(Item item, bool includeCatalog, bool includeFriendlyName)
        {
            return BuildBreadcrumbCategoryUrl(item, includeCatalog, includeFriendlyName, CatalogFoundationConstants.Routes.CategoryUrlRoute);
        }

        protected virtual string BuildBreadcrumbCategoryUrl(Item item, bool includeCatalog, bool includeFriendlyName, string root)
        {
            Assert.ArgumentNotNull(item, nameof(item));

            string catalogName = ExtractCatalogName(item, includeCatalog);
            var categoryBreadcrumbList = GetCategoryBreadcrumbList(item);

            return BuildBreadcrumbCategoryUrl(categoryBreadcrumbList, includeFriendlyName, catalogName, root);
        }

        protected virtual string BuildBreadcrumbCategoryUrl(List<Item> categories, bool includeFriendlyName, string catalogName, string root)
        {
            Assert.ArgumentNotNull(categories, nameof(categories));

            var stringBuilder = new StringBuilder("/");
            if (IncludeLanguage)
            {
                stringBuilder.Append(Context.Language.Name);
                stringBuilder.Append("/");
            }
            var giftCardProductId = StorefrontContext.CurrentStorefront.GiftCardProductId;

            if (!string.IsNullOrEmpty(catalogName))
            {
                stringBuilder.Append(EncodeUrlToken(catalogName, true));
                stringBuilder.Append("/");
            }
            stringBuilder.Append(root);

            var itemName = string.Empty;
            var itemFriendlyName = string.Empty;
            foreach (var category in categories)
            {
                stringBuilder.Append("/");
                ExtractCatalogItemInfo(category, includeFriendlyName, out itemName, out itemFriendlyName);
                if (!string.IsNullOrEmpty(itemFriendlyName))
                {
                    stringBuilder.Append(EncodeUrlToken(itemFriendlyName, true));
                    stringBuilder.Append(UrlTokenDelimiterEncoded);
                }

                itemName = RemoveCatalogFromItemName(root, itemName);
                stringBuilder.Append(EncodeUrlToken(itemName, false));
            }

            return StorefrontContext.StorefrontUri(stringBuilder.ToString()).Path;
        }

        protected virtual List<Item> GetCategoryBreadcrumbList(Item item)
        {
            var categoryBreadcrumbList = new List<Item>();
            var startNavigationCategoryID = StorefrontContext.CurrentStorefront.GetStartNavigationCategory();

            while (item.ID != startNavigationCategoryID)
            {
                categoryBreadcrumbList.Add(item);
                item = item.Parent;
            }
            categoryBreadcrumbList.Reverse();

            return categoryBreadcrumbList;
        }

        protected virtual string RemoveCatalogFromItemName(string root, string itemName)
        {
            if (root == CatalogFoundationConstants.Routes.CategoryUrlRoute)
            {
                var tokens = itemName.Split('-');
                if (tokens.Length > 1)
                {
                    itemName = tokens[1];
                }
            }

            return itemName;
        }

        protected override string EncodeUrlToken(string urlToken, bool removeInvalidPathCharacters)
        {
            if (!string.IsNullOrEmpty(urlToken))
            {
                if (removeInvalidPathCharacters)
                {
                    foreach (string invalidPathCharacter in _invalidPathCharacters)
                    {
                        urlToken = urlToken.Replace(invalidPathCharacter, string.Empty);
                    }
                }
                EncodingTokenList.ForEach(t => urlToken = urlToken.Replace(t.Delimiter, t.EncodedDelimiter));
                urlToken = urlToken.Replace(' ', '-');
                urlToken = Uri.EscapeDataString(urlToken).Replace(UrlTokenDelimiter, EncodedDelimiter);
            }

            return urlToken;
        }

        protected override string DecodeUrlToken(string urlToken)
        {
            if (!string.IsNullOrEmpty(urlToken))
            {
                urlToken = Uri.UnescapeDataString(urlToken).Replace(EncodedDelimiter, UrlTokenDelimiter);
                urlToken = urlToken.Replace('-', ' ');
                EncodingTokenList.ForEach(t => urlToken = urlToken.Replace(t.EncodedDelimiter, t.Delimiter));
            }

            return urlToken;
        }

        protected override void ExtractCatalogItemInfo(Item item, bool includeFriendlyName, out string itemName, out string itemFriendlyName)
        {
            base.ExtractCatalogItemInfo(item, includeFriendlyName, out itemName, out itemFriendlyName);
            var parentItemName = item.Parent.Name.ToLowerInvariant();
            if (itemName.StartsWith(parentItemName))
            {
                itemName = itemName.Substring(parentItemName.Length);
            }
        }
    }
}