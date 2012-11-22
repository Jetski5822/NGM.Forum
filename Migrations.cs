using System;
using Contrib.ImportExport.Services;
using NGM.Forum.Extensions;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace NGM.Forum {
    public class Migrations : DataMigrationImpl {
        private readonly ITaxonomyImportService _taxonomyImportService;

        public Migrations(ITaxonomyImportService taxonomyImportService) {
            _taxonomyImportService = taxonomyImportService;
        }

        public int Create() {
            SchemaBuilder.CreateTable("ForumPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Description", column => column.Unlimited())
                    .Column<int>("ThreadCount")
                    .Column<int>("PostCount")
                );

            SchemaBuilder.CreateTable("ThreadPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("PostCount")
                    .Column<bool>("IsSticky")
                    .Column<int>("ClosedById")
                    .Column<DateTime>("ClosedOnUtc", column => column.WithDefault(null))
                    .Column<string>("ClosedDescription", column => column.Unlimited())
                );

            SchemaBuilder.CreateTable("PostPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column<int>("RepliedOn", column => column.WithDefault(null))
                    .Column<string>("Text", column => column.Unlimited())
                    .Column<string>("Format")
                );

            var categoryTaxonomyPart = _taxonomyImportService.CreateTaxonomy(Constants.Taxonomies.Categories);
            var tagsTaxonomyPart = _taxonomyImportService.CreateTaxonomy(Constants.Taxonomies.Tags);

            ContentDefinitionManager.AlterPartDefinition("ForumPart", builder => builder
                .WithField(Constants.Taxonomies.Categories, cfg => cfg
                    .OfType("TaxonomyField")
                    .WithSetting("TaxonomyFieldSettings.AllowCustomTerms", "false")
                    .WithSetting("TaxonomyFieldSettings.SingleChoice", "true")
                    .WithSetting("TaxonomyFieldSettings.Required", "true")
                    .WithSetting("TaxonomyFieldSettings.Taxonomy", categoryTaxonomyPart.Name))
                .WithField(Constants.Taxonomies.Tags, cfg => cfg
                    .OfType("TaxonomyField")
                    .WithSetting("TaxonomyFieldSettings.AllowCustomTerms", "true")
                    .WithSetting("TaxonomyFieldSettings.Autocomplete", "true")
                    .WithSetting("TaxonomyFieldSettings.Required", "false")
                    .WithSetting("TaxonomyFieldSettings.Taxonomy", tagsTaxonomyPart.Name))
            );

            ContentDefinitionManager.AlterPartDefinition("ThreadPart", builder => builder
                .WithField(Constants.Taxonomies.Categories, cfg => cfg
                    .OfType("TaxonomyField")
                    .WithSetting("TaxonomyFieldSettings.AllowCustomTerms", "false")
                    .WithSetting("TaxonomyFieldSettings.SingleChoice", "true")
                    .WithSetting("TaxonomyFieldSettings.Required", "true")
                    .WithSetting("TaxonomyFieldSettings.Taxonomy", categoryTaxonomyPart.Name))
                .WithField(Constants.Taxonomies.Tags, cfg => cfg
                    .OfType("TaxonomyField")
                    .WithSetting("TaxonomyFieldSettings.AllowCustomTerms", "true")
                    .WithSetting("TaxonomyFieldSettings.Autocomplete", "true")
                    .WithSetting("TaxonomyFieldSettings.Required", "false")
                    .WithSetting("TaxonomyFieldSettings.Taxonomy", tagsTaxonomyPart.Name))
            );

            ContentDefinitionManager.AlterTypeDefinition("Forum", cfg => cfg
                .WithPart("ForumPart")
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-forum'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .WithPart("MenuPart")
            );

            ContentDefinitionManager.AlterTypeDefinition("Thread", cfg => cfg
                .WithPart("ThreadPart")
                .WithPart("CommonPart", builder => builder
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("ModerationPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "false")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Forum and Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum/my-thread'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
            );

            ContentDefinitionManager.AlterTypeDefinition("Post", cfg => cfg
                .WithPart("PostPart")
                .WithPart("ModerationPart")
                .WithPart("CommonPart", builder => builder
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
            );

            return 1;
        }
    }
}