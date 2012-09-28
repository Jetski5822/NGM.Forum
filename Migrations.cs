using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace NGM.Forum {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ForumPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("IsClosed")
                    .Column<int>("ThreadCount")
                    .Column<int>("PostCount")
                    .Column<bool>("UsePopularityAlgorithm")
                );

            SchemaBuilder.CreateTable("ThreadPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("IsSticky")
                    .Column<bool>("IsClosed")
                    .Column<int>("PostCount")
                );

            SchemaBuilder.CreateTable("PostPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("ParentPostId")
                    .Column<string>("Text", column => column.Unlimited())
                    .Column<string>("Format")
                );

            ContentDefinitionManager.AlterTypeDefinition("Forum",
                cfg => cfg
                    .WithPart("ForumPart")
                    .WithPart("CommonPart")
                    .WithPart("BodyPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: 'Forum/{Content.Slug}', Description: 'my-forum'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                    .WithPart("MenuPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("Thread",
                cfg => cfg
                    .WithPart("ThreadPart")
                    .WithPart("CommonPart")
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
                    .WithPart("CommonPart")
                    .WithPart("TitlePart")
                );

            return 1;
        }
    }
}