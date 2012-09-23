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
                    .Column<int>("Position")
                );

            SchemaBuilder.CreateTable("ThreadPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("IsSticky")
                    .Column<bool>("IsClosed")
                    .Column<bool>("IsAnswered")
                    .Column<int>("PostCount")
                    .Column<int>("Type")
                );

            SchemaBuilder.CreateTable("PostPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("ParentPostId")
                    .Column<int>("IsAnswer")
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
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Forum and Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum/my-thread'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                );

            ContentDefinitionManager.AlterTypeDefinition("Post",
                cfg => cfg
                    .WithPart("PostPart")
                    .WithPart("CommonPart")
                );

            return 10;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("PostPartRecord", t => t.AddColumn<int>("IsAnswer"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ThreadPartRecord", t => t.AddColumn<int>("NumberOfViews"));

            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ForumPartRecord", t => t.AddColumn<bool>("UsePopularityAlgorithm"));

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("ThreadPartRecord", t => t.DropColumn("NumberOfViews"));

            return 5;            
        }

        public int UpdateFrom5() {
            ContentDefinitionManager.AlterTypeDefinition("Thread", cfg => cfg.WithPart("UserViewPart", p => p.WithSetting("UserViewTypePartSettings.DisplayType", "Detail")));

            return 6;
        }

        public int UpdateFrom6() {
            ContentDefinitionManager.AlterTypeDefinition("Thread", cfg => cfg.RemovePart("UserViewPart"));

            SchemaBuilder.AlterTable("ForumPartRecord", t => t.AddColumn<int>("Position"));
  
            return 7;
        }

        public int UpdateFrom7() {
            ContentDefinitionManager.AlterTypeDefinition("Forum",
                cfg => cfg
                    .RemovePart("RoutePart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-forum'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")));

            ContentDefinitionManager.AlterTypeDefinition("Thread",
                cfg => cfg
                    .RemovePart("RoutePart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Forum and Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum/my-thread'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")));
                        

            return 8;
        }

        public int UpdateFrom8()
        {
            ContentDefinitionManager.AlterTypeDefinition("Forum",
                cfg => cfg
                    .RemovePart("AutoroutePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: 'Forum/{Content.Slug}', Description: 'my-forum'}]")
                        .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")));


            return 9;
        }

        public int UpdateFrom9() {
            ContentDefinitionManager.AlterTypeDefinition("Post",
                cfg => cfg
                    .RemovePart("BodyPart")
                );

            SchemaBuilder.AlterTable("PostPartRecord", t => t.AddColumn<string>("Text", column => column.Unlimited()));
            SchemaBuilder.AlterTable("PostPartRecord", t => t.AddColumn<string>("Format"));

            return 10;
        }
    }
}