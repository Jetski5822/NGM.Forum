using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace NGM.Forum {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ForumPartRecord",
                table => table
                    .ContentPartRecord()
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
                    .ContentPartRecord()
                    .Column<int>("ParentPostId")
                );

            ContentDefinitionManager.AlterTypeDefinition("Forum",
                cfg => cfg
                    .WithPart("ForumPart")
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("BodyPart")
                    .WithPart("MenuPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("Thread",
                cfg => cfg
                    .WithPart("ThreadPart")
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                );

            ContentDefinitionManager.AlterTypeDefinition("Post",
                cfg => cfg
                    .WithPart("PostPart")
                    .WithPart("CommonPart")
                    .WithPart("BodyPart")
                );

            return 1;
        }
    }
}