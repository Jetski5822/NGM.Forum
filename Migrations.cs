using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Localization;

namespace NGM.Forum {
    public class Migrations : DataMigrationImpl {
        public Migrations() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public int Create() {
            SchemaBuilder.CreateTable("ForumPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("Description", column => column.Unlimited())
                    .Column<int>("ThreadCount")
                    .Column<int>("PostCount")
                    .Column<bool>("ThreadedPosts")
                    .Column<int>("Weight")
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

            ContentDefinitionManager.AlterPartDefinition("ForumPart", builder => builder
                .Attachable()
                .WithDescription(T("Create your own Forum Type with a hieracrchy of different threads/posts").Text));

            ContentDefinitionManager.AlterPartDefinition("ThreadPart", builder => builder
                .Attachable()
                .WithDescription(T("Create your own Thread Type, useful when wanting different types of threads for different forums").Text));

            ContentDefinitionManager.AlterPartDefinition("PostPart", builder => builder
                .Attachable()
                .WithDescription(T("Create your own Post Type, useful when wanting different types of posts for different forums").Text));

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
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "false")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Forum and Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum/my-thread'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
            );

            ContentDefinitionManager.AlterTypeDefinition("Post", cfg => cfg
                .WithPart("PostPart")
                .WithPart("CommonPart", builder => builder
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
            );

            return 3;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("ForumPartRecord", command => command.AddColumn<int>("Weight"));

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("ForumPart", builder => builder
                .Attachable()
                .WithDescription(T("Create your own Forum Type with a hieracrchy of different threads/posts").Text));

            ContentDefinitionManager.AlterPartDefinition("ThreadPart", builder => builder
                .Attachable()
                .WithDescription(T("Create your own Thread Type, useful when wanting different types of threads for different forums").Text));

            ContentDefinitionManager.AlterPartDefinition("PostPart", builder => builder
                .Attachable()
                .WithDescription(T("Create your own Post Type, useful when wanting different types of posts for different forums").Text));


            return 3;
        }
    }
}