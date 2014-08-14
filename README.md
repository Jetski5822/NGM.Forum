NGM.Forum
=========

Provides Orchard CMS with a bare bones fully working forum

Original Documentation: https://github.com/Jetski5822/NGM.Forum/wiki
New Documentation: Not yet sorry.

==================

Advancing the forum functionality to include
  
  - multiple forums on the same site each with their own landing page
  - on the landing page the forums are grouped by categories with editable descriptions
  - post editing and history viewing
  - reply with quote
  - read/unread status on threads per user
  - innapropriate post reporting and management
  - antispam integration
  - datetime displays localized to the browser's time or the user's selected time zone
    - user's selected time zone requires implementing an interface the retrieves the user's preferred timezone from your own implementation
  - subscriptions to threads 
    - including customizable template for subscription notification emails 
	  - template is translatable to user's preferred language using a po.  
	  - 'preferred language' requires implementing an interface the hooks into your own 'user prefered language' implementation
  - discussion list ( threads participated in )
  - management of the forums on the front-end instead of the dashboard (so users that are moderators do not need dashboard access)
  - forum search
  - breadcrumb navigation
  - change url to be the hierachy of the titles (i.e. /forum-home-name/category-name/forum-name/post-name ) 
		-each level can be truncated to a set number of characters via settings
  - html sanitization
  - view recent posts
  - mark all posts read


  TODO:
  https://github.com/jon123/NGM.Forum/wiki/TODO

  Vocabulary
   - forums' homepage  -> the landing page for a group of forums such as 'Community Forums' or 'Tech Support Forums'
   - forum category -> a category  that groups related forums on the landing page
   - forum ->  a general topic where users posts their questions/discussions
   - post -> a question or discussion topic created by a user




 
