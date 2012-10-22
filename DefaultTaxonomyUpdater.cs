using System;
using Contrib.ImportExport.Services;
using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using NGM.Forum.Extensions;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;

namespace NGM.Forum {
    [UsedImplicitly]
    public class DefaultTaxonomyUpdater : IFeatureEventHandler {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyImportService _taxonomyImportService;

        public DefaultTaxonomyUpdater(ITaxonomyService taxonomyService,
            ITaxonomyImportService taxonomyImportService) {
            _taxonomyService = taxonomyService;
            _taxonomyImportService = taxonomyImportService;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        void IFeatureEventHandler.Installing(Feature feature)
        {
        }

        void IFeatureEventHandler.Installed(Feature feature)
        {
            AddDefaultCategoriesForFeature(feature);
            AddDefaultTagsForFeature(feature);
        }

        void IFeatureEventHandler.Enabling(Feature feature)
        {
        }

        void IFeatureEventHandler.Enabled(Feature feature)
        {
        }

        void IFeatureEventHandler.Disabling(Feature feature)
        {
        }

        void IFeatureEventHandler.Disabled(Feature feature)
        {
        }

        void IFeatureEventHandler.Uninstalling(Feature feature)
        {
        }

        void IFeatureEventHandler.Uninstalled(Feature feature)
        {
        }

        public void AddDefaultCategoriesForFeature(Feature feature) {
            var featureName = feature.Descriptor.Id;

            if (!featureName.Equals(Constants.LocalArea, StringComparison.OrdinalIgnoreCase))
                return;

            var taxonomy = _taxonomyService.GetTaxonomyByName(Constants.Taxonomies.Categories);
            _taxonomyImportService.CreateTermFor(taxonomy, "Administration", null);
            _taxonomyImportService.CreateTermFor(taxonomy, "Announcements", null);
            _taxonomyImportService.CreateTermFor(taxonomy, "General", null);
        }

        public void AddDefaultTagsForFeature(Feature feature) {
            var featureName = feature.Descriptor.Id;

            if (!featureName.Equals(Constants.LocalArea, StringComparison.OrdinalIgnoreCase))
                return;

            var taxonomy = _taxonomyService.GetTaxonomyByName(Constants.Taxonomies.Tags);
            _taxonomyImportService.CreateTermFor(taxonomy, "Question", null);
        }
    }
}