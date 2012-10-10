using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace NGM.Forum {
    public class Migrations : DataMigrationImpl {

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
                );

            SchemaBuilder.CreateTable("PostPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column<int>("ParentPostId")
                    .Column<string>("Text", column => column.Unlimited())
                    .Column<string>("Format")
                );

            ContentDefinitionManager.AlterTypeDefinition("Forum",
                cfg => cfg
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

            ContentDefinitionManager.AlterTypeDefinition("Thread",
                cfg => cfg
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

            ContentDefinitionManager.AlterTypeDefinition("Post",
                cfg => cfg
                    .WithPart("PostPart")
                    .WithPart("CommonPart", builder => builder
                        .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("UserForumPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("RequiresModeration")
                );

            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart("UserForumPart")
                );

            SchemaBuilder.AlterTable("PostPartRecord", table => table.AddColumn<bool>("RequiresModeration"));

            return 2;
        }

        /* A Rule should be defined to switch 'RequiresModeration' against the user' off once a number of posts has been created, etc... */
    }
}