using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Core.Title.Models;

namespace Contrib.ImportExport.Services {
    public interface ITaxonomyImportService : IDependency {
        TaxonomyPart CreateTaxonomy(string taxonomyName);
        TermPart CreateTermFor(TaxonomyPart taxonomy, string termName, string termSlug);
    }

    public class TaxonomyImportService : ITaxonomyImportService {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IMembershipService _membershipService;

        public TaxonomyImportService(ISiteService siteService, IContentManager contentManager, ITaxonomyService taxonomyService, IMembershipService membershipService) {
            _siteService = siteService;
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
            _membershipService = membershipService;
        }

        public TaxonomyPart CreateTaxonomy(string taxonomyName) {
            var existingTaxonomy = _taxonomyService.GetTaxonomyByName(taxonomyName);
            if (existingTaxonomy != null)
                return existingTaxonomy;

            var taxonomy = _contentManager.New<TaxonomyPart>("Taxonomy");
            taxonomy.As<ICommonPart>().Owner = _membershipService.GetUser(_siteService.GetSiteSettings().SuperUser);

            taxonomy.Slug = taxonomyName.ToLowerInvariant();
            taxonomy.As<TitlePart>().Title = taxonomyName;

            _contentManager.Create(taxonomy, VersionOptions.Published);
            _taxonomyService.CreateTermContentType(taxonomy.As<TaxonomyPart>());

            return taxonomy.As<TaxonomyPart>();
        }

        public TermPart CreateTermFor(TaxonomyPart taxonomy, string termName, string termSlug) {
            var term = _taxonomyService.NewTerm(taxonomy);

            term.Weight = 0;
            term.Container = taxonomy.ContentItem;
            term.Name = termName.Trim();

            if (!string.IsNullOrEmpty(termSlug) || !string.IsNullOrWhiteSpace(termSlug))
                term.Slug = termSlug.Trim();
            
            //TODO: Commented this one - not sure if it's necessary or not
            //_routableService.ProcessSlug(term.As<IRoutableAspect>());
            _taxonomyService.ProcessPath(term);
            _contentManager.Create(term, VersionOptions.Published);

            return term;
        }
    }
}
