'
' DotNetNuke� - http://www.dotnetnuke.com
' Copyright (c) 2002-2010
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Namespace DotNetNuke.Modules.Forum

	''' <summary>
	''' The forum Info object uses caching and handles the population of all 
	''' data specific to a single forum instance.  Rendering for Group view 
	''' items is also handled here.
	''' </summary>
	''' <remarks>
	''' </remarks>
	Public Class ForumInfo

#Region "Private Members"

		Dim _GroupID As Integer
		Dim _ModuleID As Integer
		Dim _ForumID As Integer
		Dim _IsActive As Boolean = True
		Dim _ParentID As Integer
		Dim _Name As String = Null.NullString
		Dim _Description As String = Null.NullString
		Dim _CreatedDate As DateTime = Null.NullDate
		Dim _CreatedByUser As Integer
		Dim _IsModerated As Boolean = False
		Dim _SortOrder As Integer
		Dim _ForumType As Integer
		Dim _IsIntegrated As Boolean
		Dim _IntegratedModuleID As Integer
		Dim _IntegratedObjects As String
		Dim _UpdatedDate As DateTime
		Dim _UpdatedByUser As Integer
		Dim _ForumPermissions As DotNetNuke.Modules.Forum.ForumPermissionCollection
		Dim _EnablePostStatistics As Boolean = True
		Dim _PublicPosting As Boolean = True
		Dim _EnableForumsThreadStatus As Boolean = True
		Dim _EnableForumsRating As Boolean = True
		Dim _ForumLink As String
		Dim _ForumBehavior As DotNetNuke.Modules.Forum.ForumBehavior
		Dim _EnableRSS As Boolean = True
		Dim _AllowPolls As Boolean = False
		Dim _SubForums As Integer
		Dim _DisablePostCount As Boolean = False
		'Anytime a post is added, these values can change
		Dim _MostRecentPostID As Integer
		Dim _MostRecentThreadID As Integer
		Dim _MostRecentPostAuthorID As Integer
		Dim _MostRecentPostDate As DateTime = Null.NullDate
		Dim _MostRecentThreadPinned As Boolean = False
		Dim _PostsToModerate As Integer
		Dim _TotalPosts As Integer
		Dim _TotalThreads As Integer
		Dim _PortalID As Integer
		' Email
		Dim _EmailAddress As String
		Dim _EmailFriendlyFrom As String
		Dim _NotifyByDefault As Boolean = False
		Dim _EmailStatusChange As Boolean = False
		Dim _EmailServer As String
		Dim _EmailUser As String
		Dim _EmailPass As String
		Dim _EmailEnableSSL As Boolean = False
		Dim _EmailAuth As Integer = 0	' 0 = none (for now, eventually need an enum)
		Dim _EmailPort As Integer = 110 ' 995 also used for gmail

#End Region

#Region "Constructors"

		''' <summary>
		''' Creates a new instance of the foruminfo object.
		''' </summary>
		''' <remarks>
		''' </remarks>
		Public Sub New()
		End Sub

#End Region

#Region "Public ReadOnly Properties"

		''' <summary>
		''' Boolean information to identify if SubForums > 0
		''' </summary>
		''' <value></value>
		''' <returns>True/False</returns>
		''' <remarks>Added by Skeel</remarks>
		Public ReadOnly Property IsParentForum() As Boolean
			Get
				If SubForums > 0 Then
					Return True
				Else
					Return False
				End If
			End Get
		End Property

		''' <summary>
		''' Determines if the forum is moderated/unmoderated. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property IsModerated() As Boolean
			Get

				If (ForumBehavior = ForumBehavior.PrivateModerated Or ForumBehavior = ForumBehavior.PrivateModeratedPostRestricted Or ForumBehavior = ForumBehavior.PublicModerated Or ForumBehavior = ForumBehavior.PublicModeratedPostRestricted) Then
					Return True
				Else
					Return False
				End If
			End Get
		End Property

		''' <summary>
		''' If the forum is private/public viewing
		''' </summary>
		''' <value></value>
		''' <returns>True if the forum allows public viewing, false otherwise.</returns>
		''' <remarks>If public, the access limits are based on the module's view permissions. Uses Enum to match saved integer value.</remarks>
		Public ReadOnly Property PublicView() As Boolean
			Get

				If (ForumBehavior = ForumBehavior.PrivateModerated Or ForumBehavior = ForumBehavior.PrivateModeratedPostRestricted Or ForumBehavior = ForumBehavior.PrivateUnModerated Or ForumBehavior = ForumBehavior.PrivateUnModeratedPostRestricted) Then
					Return False
				Else
					Return True
				End If
			End Get
		End Property

		''' <summary>
		''' If public posting is permitted. If not, there are posting restrictions set by roles. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property PublicPosting() As Boolean
			Get
				If (ForumBehavior = ForumBehavior.PrivateModeratedPostRestricted Or ForumBehavior = ForumBehavior.PrivateUnModeratedPostRestricted Or ForumBehavior = ForumBehavior.PublicModeratedPostRestricted Or ForumBehavior = ForumBehavior.PublicUnModeratedPostRestricted) Then
					Return False
				Else
					Return True
				End If
			End Get
		End Property

		''' <summary>
		''' The PortalID the forum belongs too. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property PortalID() As Integer
			Get
				Return ParentGroup.PortalID
			End Get
		End Property

		''' <summary>
		''' The group information which contains this forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property ParentGroup() As GroupInfo
			Get
				If GroupID <> -1 Then
					Return GroupInfo.GetGroupInfo(GroupID)
				Else
					Dim _groupInfo As GroupInfo = New GroupInfo
					_groupInfo.ModuleID = ModuleID
					_groupInfo.GroupID = GroupID
					Return _groupInfo
				End If
			End Get
		End Property

		''' <summary>
		''' Attaches an instance of a parent forum object to the current forum. 
		''' </summary>
		''' <value></value>
		''' <returns>The parent forum object if ParentID > 0, otherwise nothing is returned</returns>
		''' <remarks></remarks>
		Public ReadOnly Property ParentForum() As ForumInfo
			Get
				Dim objForum As ForumInfo = New ForumInfo

				If ParentId > 0 Then
					Dim cntForum As New ForumController
					objForum = cntForum.GetForumInfoCache(ParentId)
				Else
					objForum.ModuleID = ModuleID
					objForum.ForumID = ForumID
				End If

				Return objForum
			End Get
		End Property

		''' <summary>
		''' The last approved post's Author's user information for this forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public ReadOnly Property MostRecentPostAuthor() As ForumUser
			Get
				Dim cntForumUser As New ForumUserController
				Return cntForumUser.GetForumUser(MostRecentPostAuthorID, False, ModuleID, PortalID)
			End Get
		End Property

#End Region

#Region "Public Properties"

		''' <summary>
		''' The GroupID of the Group this forum is part of.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property GroupID() As Integer
			Get
				Return _GroupID
			End Get
			Set(ByVal Value As Integer)
				_GroupID = Value
			End Set
		End Property

		''' <summary>
		''' The ModuleID this forum is related to. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ModuleID() As Integer
			Get
				Return _ModuleID
			End Get
			Set(ByVal Value As Integer)
				_ModuleID = Value
			End Set
		End Property

		''' <summary>
		''' The ForumID of the current forum
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ForumID() As Integer
			Get
				Return _ForumID
			End Get
			Set(ByVal Value As Integer)
				_ForumID = Value
			End Set
		End Property

		''' <summary>
		''' If the forum is enabled/disabled.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IsActive() As Boolean
			Get
				Return _IsActive
			End Get
			Set(ByVal Value As Boolean)
				_IsActive = Value
			End Set
		End Property

		''' <summary>
		''' The parentForumID should be 0 for now.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ParentId() As Integer
			Get
				Return _ParentID
			End Get
			Set(ByVal Value As Integer)
				_ParentID = Value
			End Set
		End Property

		''' <summary>
		''' The name the users will see for the forum.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property Name() As String
			Get
				Return _Name
			End Get
			Set(ByVal Value As String)
				_Name = Value
			End Set
		End Property

		''' <summary>
		''' A brief description of the forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property Description() As String
			Get
				Return _Description
			End Get
			Set(ByVal Value As String)
				_Description = Value
			End Set
		End Property

		''' <summary>
		''' The UserID of the user who created the forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property CreatedByUser() As Integer
			Get
				Return _CreatedByUser
			End Get
			Set(ByVal Value As Integer)
				_CreatedByUser = Value
			End Set
		End Property

		''' <summary>
		''' The date the forum was created. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property CreatedDate() As DateTime
			Get
				Return _CreatedDate
			End Get
			Set(ByVal Value As DateTime)
				_CreatedDate = Value
			End Set
		End Property

		''' <summary>
		''' The order (ascending/descending) the threads should be displayed. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property SortOrder() As Integer
			Get
				Return _SortOrder
			End Get
			Set(ByVal Value As Integer)
				_SortOrder = Value
			End Set
		End Property

		''' <summary>
		''' Total number of approved posts in the forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property TotalPosts() As Integer
			Get
				Return _TotalPosts
			End Get
			Set(ByVal Value As Integer)
				_TotalPosts = Value
			End Set
		End Property

		''' <summary>
		''' Total number of approved threads in the forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property TotalThreads() As Integer
			Get
				Return _TotalThreads
			End Get
			Set(ByVal Value As Integer)
				_TotalThreads = Value
			End Set
		End Property

		''' <summary>
		''' If post stats are fed to a queue (non-functional)
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EnablePostStatistics() As Boolean
			Get
				Return _EnablePostStatistics
			End Get
			Set(ByVal Value As Boolean)
				_EnablePostStatistics = Value
			End Set
		End Property

		''' <summary>
		''' The last approved post in a forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property MostRecentPostID() As Integer
			Get
				Return _MostRecentPostID
			End Get
			Set(ByVal Value As Integer)
				_MostRecentPostID = Value
			End Set
		End Property

		''' <summary>
		''' The last approved thread in a forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property MostRecentThreadID() As Integer
			Get
				Return _MostRecentThreadID
			End Get
			Set(ByVal Value As Integer)
				_MostRecentThreadID = Value
			End Set
		End Property

		''' <summary>
		''' The last approved post author's UserID. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property MostRecentPostAuthorID() As Integer
			Get
				Return _MostRecentPostAuthorID
			End Get
			Set(ByVal Value As Integer)
				_MostRecentPostAuthorID = Value
			End Set
		End Property

		''' <summary>
		''' The last approved posts date. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property MostRecentPostDate() As DateTime
			Get
				Return _MostRecentPostDate
			End Get
			Set(ByVal Value As DateTime)
				_MostRecentPostDate = Value
			End Set
		End Property

		''' <summary>
		''' Determines if the most recent thread is pinned.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property MostRecentThreadPinned() As Boolean
			Get
				Return _MostRecentThreadPinned
			End Get
			Set(ByVal Value As Boolean)
				_MostRecentThreadPinned = Value
			End Set
		End Property

		''' <summary>
		''' Number of posts in the moderation queue for this forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property PostsToModerate() As Integer
			Get
				Return _PostsToModerate
			End Get
			Set(ByVal Value As Integer)
				_PostsToModerate = Value
			End Set
		End Property

		''' <summary>
		''' The type of forum determines how it will behave. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ForumType() As Integer
			Get
				Return _ForumType
			End Get
			Set(ByVal Value As Integer)
				_ForumType = Value
			End Set
		End Property

		''' <summary>
		''' If the forum is integrated w/ a 3rd party module
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IsIntegrated() As Boolean
			Get
				Return _IsIntegrated
			End Get
			Set(ByVal Value As Boolean)
				_IsIntegrated = Value
			End Set
		End Property

		''' <summary>
		''' 3rd party integration Module's ModuleID. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IntegratedModuleID() As Integer
			Get
				Return _IntegratedModuleID
			End Get
			Set(ByVal Value As Integer)
				_IntegratedModuleID = Value
			End Set
		End Property

		''' <summary>
		''' The objects which are integrated (possibly legacy)
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property IntegratedObjects() As String
			Get
				Return _IntegratedObjects
			End Get
			Set(ByVal Value As String)
				_IntegratedObjects = Value
			End Set
		End Property

		''' <summary>
		''' The UserID of the person to last update the forum settings. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property UpdatedByUser() As Integer
			Get
				Return _UpdatedByUser
			End Get
			Set(ByVal Value As Integer)
				_UpdatedByUser = Value
			End Set
		End Property

		''' <summary>
		''' The date the forum settings were last updated. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property UpdatedDate() As DateTime
			Get
				Return _UpdatedDate
			End Get
			Set(ByVal Value As DateTime)
				_UpdatedDate = Value
			End Set
		End Property

		''' <summary>
		''' The url which represents the forum link.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>This is used only for Link forum types.</remarks>
		Public Property ForumLink() As String
			Get
				Return _ForumLink
			End Get
			Set(ByVal Value As String)
				_ForumLink = Value
			End Set
		End Property

		''' <summary>
		''' Uses an enum to determine public/private, restricted posting/not restricted, moderated/not moderated.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ForumBehavior() As DotNetNuke.Modules.Forum.ForumBehavior
			Get
				Return _ForumBehavior
			End Get
			Set(ByVal Value As DotNetNuke.Modules.Forum.ForumBehavior)
				_ForumBehavior = Value
			End Set
		End Property

		''' <summary>
		''' A collection of forum permissions for a single forumID instance. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property ForumPermissions() As DotNetNuke.Modules.Forum.ForumPermissionCollection
			Get
				Return _ForumPermissions
			End Get
			Set(ByVal Value As DotNetNuke.Modules.Forum.ForumPermissionCollection)
				_ForumPermissions = Value
			End Set
		End Property

		''' <summary>
		''' If thread status feature is enabled for this forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EnableForumsThreadStatus() As Boolean
			Get
				Return _EnableForumsThreadStatus
			End Get
			Set(ByVal Value As Boolean)
				_EnableForumsThreadStatus = Value
			End Set
		End Property

		''' <summary>
		''' If rating capability is enabled for this particular forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EnableForumsRating() As Boolean
			Get
				Return _EnableForumsRating
			End Get
			Set(ByVal Value As Boolean)
				_EnableForumsRating = Value
			End Set
		End Property

		''' <summary>
		''' If the forum allows polls to be created.
		''' </summary>
		''' <value></value>
		''' <returns>True if polls are enabled, false otherwise.</returns>
		Public Property AllowPolls() As Boolean
			Get
				Return _AllowPolls
			End Get
			Set(ByVal Value As Boolean)
				_AllowPolls = Value
			End Set
		End Property

		''' <summary>
		''' Permits enable/disable of RSS feeds per forum. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EnableRSS() As Boolean
			Get
				Return _EnableRSS
			End Get
			Set(ByVal Value As Boolean)
				_EnableRSS = Value
			End Set
		End Property

		''' <summary>
		''' Counts subforums
		''' </summary>
		''' <value></value>
		''' <returns>Number of subforums</returns>
		Public Property SubForums() As Integer
			Get
				Return _SubForums
			End Get
			Set(ByVal Value As Integer)
				_SubForums = Value
			End Set
		End Property

		''' <summary>
		''' The FROM email addressed used for outgoing emails. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailAddress() As String
			Get
				Return _EmailAddress
			End Get
			Set(ByVal Value As String)
				_EmailAddress = Value
			End Set
		End Property

		''' <summary>
		''' The friendly name to use instead of the actual address. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>This helps reduce the chance of the message being flagged as SPAM.</remarks>
		Public Property EmailFriendlyFrom() As String
			Get
				Return _EmailFriendlyFrom
			End Get
			Set(ByVal Value As String)
				_EmailFriendlyFrom = Value
			End Set
		End Property

		''' <summary>
		''' If users should be subscribed to a forum by default (only happens user's first visit)
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Not implemented yet.</remarks>
		Public Property NotifyByDefault() As Boolean
			Get
				Return _NotifyByDefault
			End Get
			Set(ByVal Value As Boolean)
				_NotifyByDefault = Value
			End Set
		End Property

		''' <summary>
		''' If user's can receive emails when a thread status change is made. 
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks>Not implemented yet.</remarks>
		Public Property EmailStatusChange() As Boolean
			Get
				Return _EmailStatusChange
			End Get
			Set(ByVal Value As Boolean)
				_EmailStatusChange = Value
			End Set
		End Property

		''' <summary>
		''' Not implemented.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailServer() As String
			Get
				Return _EmailServer
			End Get
			Set(ByVal Value As String)
				_EmailServer = Value
			End Set
		End Property

		''' <summary>
		''' Not implemented.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailUser() As String
			Get
				Return _EmailUser
			End Get
			Set(ByVal Value As String)
				_EmailUser = Value
			End Set
		End Property

		''' <summary>
		''' Not implemented.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailPass() As String
			Get
				Return _EmailPass
			End Get
			Set(ByVal Value As String)
				_EmailPass = Value
			End Set
		End Property

		''' <summary>
		''' Not implemented.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailEnableSSL() As Boolean
			Get
				Return _EmailEnableSSL
			End Get
			Set(ByVal Value As Boolean)
				_EmailEnableSSL = Value
			End Set
		End Property

		''' <summary>
		''' Not implemented.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailAuth() As Integer
			Get
				Return _EmailAuth
			End Get
			Set(ByVal Value As Integer)
				_EmailAuth = Value
			End Set
		End Property

		''' <summary>
		''' The port that will be used to check email.
		''' </summary>
		''' <value></value>
		''' <returns></returns>
		''' <remarks></remarks>
		Public Property EmailPort() As Integer
			Get
				Return _EmailPort
			End Get
			Set(ByVal Value As Integer)
				_EmailPort = Value
			End Set
		End Property

#End Region

	End Class

End Namespace