using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.Autoroute.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Indexing;
using Orchard.Logging;
using NGM.Forum.Services;
using Orchard.Indexing.Settings;
using Orchard.Indexing.Services;

namespace NGM.Forum {
    public class Migrations : DataMigrationImpl {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<SubscriptionEmailTemplateRecord> _subscriptionEmailTemplateRepository;
        private readonly IIndexManager _indexManager;
        private readonly IIndexingService _indexingService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ILogger Logger { get; set; }


        public Migrations(
            IOrchardServices orchardServices,
            IRepository<SubscriptionEmailTemplateRecord> subscriptionEmailTemplateRepository,
            IIndexManager indexManager,
            IIndexingService indexingService,
            IContentDefinitionManager contentDefinitionManager
        )
        {
            _orchardServices = orchardServices;
            _subscriptionEmailTemplateRepository = subscriptionEmailTemplateRepository;
            _indexManager = indexManager;
            _indexingService = indexingService;
            _contentDefinitionManager = contentDefinitionManager;
            Logger = NullLogger.Instance;
            
        }

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
                .WithDescription("Create your own Forum Type with a hierarchy of different threads/posts"));

            ContentDefinitionManager.AlterPartDefinition("ThreadPart", builder => builder
                .Attachable()
                .WithDescription("Create your own Thread Type, useful when wanting different types of threads for different forums"));

            ContentDefinitionManager.AlterPartDefinition("PostPart", builder => builder
                .Attachable()
                .WithDescription("Create your own Post Type, useful when wanting different types of posts for different forums"));

            ContentDefinitionManager.AlterTypeDefinition("Forum", cfg => cfg
                .WithPart("ForumPart")
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum'}]")
                    // .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-forum'}]")
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

            return 1;
        }

        public int UpdateFrom1()
        {
            //SchemaBuilder.AlterTable("ForumPartRecord", command => command.AddColumn<int>("Weight"));

            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterPartDefinition("ForumPart", builder => builder
                .Attachable()
                .WithDescription("Create your own Forum Type with a hierarchy of different threads/posts"));

            ContentDefinitionManager.AlterPartDefinition("ThreadPart", builder => builder
                .Attachable()
                .WithDescription("Create your own Thread Type, useful when wanting different types of threads for different forums"));

            ContentDefinitionManager.AlterPartDefinition("PostPart", builder => builder
                .Attachable()
                .WithDescription("Create your own Post Type, useful when wanting different types of posts for different forums"));


            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.CreateTable("ThreadSubscriptionRecord",
                 table => table
                     .Column<int>("Id", c => c.PrimaryKey().Identity())
                     .Column<int>("ThreadId")
                     .Column<int>("UserId")
                     .Column<DateTime>("SubscribedDate", column => column.Nullable())
                     .Column<bool>("EmailUpdates")
             );

            SchemaBuilder.CreateTable("ThreadLastReadRecord",
                 table => table
                     .Column<int>("Id", c => c.PrimaryKey().Identity())
                     .Column<int>("ThreadId")
                     .Column<int>("UserId")
                     .Column<DateTime>("LastReadDate", column => column.Nullable())
             );

            SchemaBuilder.AlterTable("PostPartRecord",
                 table => table
                     .AddColumn<DateTime>("LastEdited", column => column.Nullable())                     
             );

            SchemaBuilder.CreateTable("ForumCategoryPartRecord",
                 table => table
                     .ContentPartVersionRecord()
                     .Column<String>("Description", c => c.Unlimited())
                     .Column<int>("Weight")
             );

            ContentDefinitionManager.AlterTypeDefinition("ForumCategory", cfg => cfg
                .WithPart("TitlePart")
                .WithPart("CommonPart", builder => builder
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                .WithPart("ForumCategoryPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "false")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Forum and Category', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum/my-thread'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
            );

            return 4;
        }

        public int UpdateFrom4()
        {

            SchemaBuilder.CreateTable("ForumsHomePagePartRecord",
              table => table
                  //TODO: review -> should this be a version record or a normal part record ?
                  .ContentPartVersionRecord()
            );

            ContentDefinitionManager.AlterPartDefinition("ForumsHomePagePart", builder => builder                
                .Attachable()
                .WithDescription("The root of the forums, representing the base URL."));

            ContentDefinitionManager.AlterTypeDefinition("ForumsHomePage", cfg => cfg
                .WithPart("CommonPart", builder => builder
                    .WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                .WithPart("TitlePart")
                .WithPart("BodyPart")
                .WithPart("ForumsHomePagePart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.AllowCustomPattern", "false")
                    .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "true")
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Forum and Category', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-forum/my-thread'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
            );
            return 5;
        }

        public int UpdateFrom5()
        {

            /* this is definitely doing it the hard way keying off of userId because the user name must be looked up afterward. 
             * however this approach (in theory) would allow users to change their username
             */
            SchemaBuilder.CreateTable("ReportedPostRecord",
              table => table
                  .Column<int>("Id", c => c.PrimaryKey().Identity())
                  .Column<int>("PostId")
                  .Column<int>("PostedByUserId") 
                  .Column<int>("ReportedByUserId")
                  .Column<DateTime>("ReportedDate")
                  .Column<bool>("IsResolved")
                  .Column<DateTime>("ResolvedDate")
                  .Column<int>("ResolvedByUserId")
                  .Column<String>("Note", c=>c.Unlimited())
            );

            return 6;
        }

        public int UpdateFrom6()
        {
            SchemaBuilder.AlterTable("PostPartRecord",
                table => table
                    .AddColumn<bool>("IsInappropriate", column => column.WithDefault(false))                    
                );

            SchemaBuilder.AlterTable("ThreadPartRecord",
                table => table.AddColumn<bool>("IsInappropriate", column => column.WithDefault(false))
                );

            return 7;
        }
        public int UpdateFrom7()
        {
            SchemaBuilder.AlterTable("PostPartRecord",
                table => table
                    .AddColumn<bool>("IsDeleted", column => column.WithDefault(false))
                );
            SchemaBuilder.AlterTable("PostPartRecord",
                table => table
                    .AddColumn<string>("IP", column => column.WithLength(46))
                );
            SchemaBuilder.AlterTable("ThreadPartRecord",
                table => table
                 .AddColumn<bool>("IsDeleted", column => column.WithDefault(false))                 
                );
            return 8;
        }
        public int UpdateFrom8()
        {
            SchemaBuilder.AlterTable("ThreadPartRecord",
                table => table
                 .AddColumn<int>("ForumsHomepageId")
             );           

            SchemaBuilder.CreateTable("PostEditHistoryRecord",
               table => table
                   .Column<int>("Id", c => c.PrimaryKey().Identity())
                   .Column<int>("PostId")
                   .Column<int>("UserId")  
                   .Column<String>("Text", col => col.Unlimited())
                   .Column<DateTime>("EditDate")
                   .Column<String>("Format")
             );

            return 9;
        }

        public int UpdateFrom9()
        {
            /*use a constraint to avoid the possibility of double subscribing to the same thread */
            /* tested on SQL Compact but not yet on sql server .. not going to be tested on mysql */
            string sql = "ALTER TABLE NGM_Forum_ThreadSubscriptionRecord ADD CONSTRAINT uniqueColumns UNIQUE (ThreadId, UserId)";
            SchemaBuilder.ExecuteSql( sql);
            return 10;
        }

        public int UpdateFrom10()
        {
            SchemaBuilder.CreateTable("ForumsSettingsPartRecord",
               table => table
                   .ContentPartRecord()
                   /* default lengths are set in the part constructor */
                   .Column<int>("ForumsHomeTitleMaximumLength")
                   .Column<int>("ForumsHomeUrlMaximumLength")
                   .Column<int>("CategoryTitleMaximumLength")
                   .Column<int>("CategoryUrlMaximumLength")
                   .Column<int>("ThreadTitleMaximumLength")
                   .Column<int>("ThreadUrlMaximumLength")
             );
            return 11;
        }


        public int UpdateFrom11()
        {
            /* 
             * The design goal for sending emails was to meet the following requirements:
             * 1) allow the 'default template' of the notification email to be translatable (i.e. a po)
             *    so that translations can be added by the community - and sendable in the user's default language
             * 2) all the user to be able to change the default email template if desired (and translate themself if necessary)
             * 3) the 'title' is plain text
             * 4) the 'body' should support a plain text and html version so it can match the user's preference for receiving email
             */
            SchemaBuilder.CreateTable("SubscriptionEmailTemplateRecord",
               table => table
                   .Column<int>("Id", c => c.PrimaryKey().Identity())
                   .Column<string>("Title", col => col.WithLength(1048))
                   .Column<string>("BodyPlainText", col => col.Unlimited())
                   .Column<string>("BodyHtml", col => col.Unlimited())
            );
            return 12;
        }
        public int UpdateFrom12()
        {
            var defaultSubscriptionEmailTemplate = new SubscriptionEmailTemplateRecord();
            defaultSubscriptionEmailTemplate.Title = "A new post was made to a forum topic you subscribed to at [SiteName]";
            defaultSubscriptionEmailTemplate.BodyPlainText = "You are receiving this email because you subscribed to receive post notifications on the thread:" + Environment.NewLine +
                                                             "'[ThreadTitle]' " + Environment.NewLine + Environment.NewLine +

                                                             "The contents of the new post are as follows:" + Environment.NewLine + Environment.NewLine +

                                                             "[PostText]" + Environment.NewLine + Environment.NewLine +

                                                             "To view this post or respond go to [PostUrl]" + Environment.NewLine;

            defaultSubscriptionEmailTemplate.BodyHtml = "<html><body>" + 
                                                        "<p>{\"You received this email because you subscribed to receive post notification on the thread:\"}</p>" +
                                                        "<p><i>[ThreadTitle]</i></p>" +
                                                        "<p>{(\"The contents of the new post is as follows:\"} </p>" +
                                                        "<p>[PostText]</p>" +
                                                        "<p>{(\"To view this post or to respond go to\"} [PostUrl]</p>" +
                                                        "</body></html>";

            _subscriptionEmailTemplateRepository.Create(defaultSubscriptionEmailTemplate);

            return 13;
        }

        public int UpdateFrom13()
        {

            SchemaBuilder.AlterTable("ReportedPostRecord",
                         table => table                             
                             .AddColumn<String>("ReasonReported", c => c.WithLength( 2048) )
                       );
            return 14;
        }

        public int UpdateFrom14()
        {
            ContentDefinitionManager.AlterTypeDefinition("ForumSearch", cfg => cfg
                .WithPart("ForumSearchPart")
                .WithPart("CommonPart"));
            return 15;
        }

        public int UpdateFrom15()
        {
            ContentDefinitionManager.AlterPartDefinition("ForumSearchPart", builder => builder
              .Attachable()
              .WithDescription("Adds a box to the content-item that searches the content-item's parent forums."));

            return 16;
        }

        public int UpdateFrom16()
        {

            return 17;
        }

        public int UpdateFrom17()
        {

            string indexName = ForumSearchService.FORUMS_INDEX_NAME;
            var provider = _indexManager.GetSearchIndexProvider();

            if (provider.Exists(indexName))
            {
                Logger.Error("When the installing the forums module the search index '{0}' already exists. Please recreate the index manually if need be.", indexName);
            }

            try
            {
                provider.CreateIndex(indexName);
            }
            catch (Exception e)
            {
                Logger.Error(e, String.Format("An error occured while creating the index: {0}", indexName));
            }

            return 18;

        }

        public int UpdateFrom18()
        {
            //setup the initial index for the psot part
            var provider = _indexManager.GetSearchIndexProvider();

            if (provider == null)
            {
                Logger.Error( "An error occured in the forum migration. A search index provider cannot be found.  Forum searching will not function properly until an indexd name {0} is created and related to the Post content item", ForumSearchService.FORUMS_INDEX_NAME );
            }

            var indexName = ForumSearchService.FORUMS_INDEX_NAME;
            provider.CreateIndex(indexName);


            var settings = _contentDefinitionManager.GetTypeDefinition("Post").Settings;
            var indexingSettings = settings.TryGetModel<TypeIndexing>();

            if (indexingSettings == null)
            {
                _contentDefinitionManager.AlterTypeDefinition("Post",
                    cfg => cfg
                        .WithSetting("TypeIndexing.Indexes", indexName)
                    );
            }
            else
            {
                _contentDefinitionManager.AlterTypeDefinition("Post",
                    cfg => cfg
                        .WithSetting("TypeIndexing.Indexes", indexingSettings.Indexes + "," + indexName)
                    );
            }
            _indexingService.RebuildIndex(indexName);

            return 19;
         }

        public int UpdateFrom19()
        {
            SchemaBuilder.AlterTable("ThreadPartRecord",
                 table => table
                     .AddColumn<DateTime>("LatestValidPostDate")
            );

            return 20;
        }

        public int UpdateFrom20()
        {
            SchemaBuilder.CreateTable("ForumsHomePageLastReadRecord",
                 table => table
                   .Column<int>("Id", c => c.PrimaryKey().Identity())
                   .Column<int>("ForumsHomePageId")
                   .Column<DateTime>("LastReadDate")
                   .Column<int>("UserId")
            );

            return 21;
        }
        public int UpdateFrom21()
        {
            SchemaBuilder.AlterTable("ForumsSettingsPartRecord",
                 table => table
                     //default to one month
                     .AddColumn<int>("DaysUntilThreadReadByDefault", column=>column.WithDefault( 30 ))
            );

            return 22;
        }
        
    }
}