using System;
using Foundation;
using ObjCRuntime;

namespace Sparkle
{

    // @interface SUAppcast : NSObject <NSURLDownloadDelegate>
    [BaseType (typeof (NSObject))]
    interface SUAppcast
    {
        // @property (copy) NSString * userAgentString;
        [Export ("userAgentString")]
        string UserAgentString { get; set; }

        // @property (copy) NSDictionary * httpHeaders;
        [Export ("httpHeaders", ArgumentSemantic.Copy)]
        NSDictionary HttpHeaders { get; set; }

        // -(void)fetchAppcastFromURL:(NSURL *)url completionBlock:(void (^)(NSError *))err;
        [Export ("fetchAppcastFromURL:completionBlock:")]
        void FetchAppcastFromURL (NSUrl url, Action<NSError> err);

        // -(SUAppcast *)copyWithoutDeltaUpdates;
        [Export ("copyWithoutDeltaUpdates")]
        SUAppcast CopyWithoutDeltaUpdates { get; }

        // @property (readonly, copy) NSArray * items;
        [Export ("items", ArgumentSemantic.Copy)]
        NSObject [] Items { get; }
    }

    // @interface SUAppcastItem : NSObject
    [BaseType (typeof (NSObject))]
    interface SUAppcastItem
    {
        // @property (readonly, copy) NSString * title;
        [Export ("title")]
        string Title { get; }

        // @property (readonly, copy) NSDate * date;
        [Export ("date", ArgumentSemantic.Copy)]
        NSDate Date { get; }

        // @property (readonly, copy) NSString * itemDescription;
        [Export ("itemDescription")]
        string ItemDescription { get; }

        // @property (readonly, strong) NSURL * releaseNotesURL;
        [Export ("releaseNotesURL", ArgumentSemantic.Strong)]
        NSUrl ReleaseNotesURL { get; }

        // @property (readonly, copy) NSString * DSASignature;
        [Export ("DSASignature")]
        string DSASignature { get; }

        // @property (readonly, copy) NSString * minimumSystemVersion;
        [Export ("minimumSystemVersion")]
        string MinimumSystemVersion { get; }

        // @property (readonly, copy) NSString * maximumSystemVersion;
        [Export ("maximumSystemVersion")]
        string MaximumSystemVersion { get; }

        // @property (readonly, strong) NSURL * fileURL;
        [Export ("fileURL", ArgumentSemantic.Strong)]
        NSUrl FileURL { get; }

        // @property (readonly, copy) NSString * versionString;
        [Export ("versionString")]
        string VersionString { get; }

        // @property (readonly, copy) NSString * displayVersionString;
        [Export ("displayVersionString")]
        string DisplayVersionString { get; }

        // @property (readonly, copy) NSDictionary * deltaUpdates;
        [Export ("deltaUpdates", ArgumentSemantic.Copy)]
        NSDictionary DeltaUpdates { get; }

        // @property (readonly, strong) NSURL * infoURL;
        [Export ("infoURL", ArgumentSemantic.Strong)]
        NSUrl InfoURL { get; }

        // -(instancetype)initWithDictionary:(NSDictionary *)dict;
        [Export ("initWithDictionary:")]
        IntPtr Constructor (NSDictionary dict);

        // -(instancetype)initWithDictionary:(NSDictionary *)dict failureReason:(NSString **)error;
        [Export ("initWithDictionary:failureReason:")]
        IntPtr Constructor (NSDictionary dict, out string error);

        // @property (readonly, getter = isDeltaUpdate) BOOL deltaUpdate;
        [Export ("deltaUpdate")]
        bool DeltaUpdate { [Bind ("isDeltaUpdate")] get; }

        // @property (readonly, getter = isCriticalUpdate) BOOL criticalUpdate;
        [Export ("criticalUpdate")]
        bool CriticalUpdate { [Bind ("isCriticalUpdate")] get; }

        // @property (readonly, getter = isInformationOnlyUpdate) BOOL informationOnlyUpdate;
        [Export ("informationOnlyUpdate")]
        bool InformationOnlyUpdate { [Bind ("isInformationOnlyUpdate")] get; }

        // @property (readonly, copy) NSDictionary * propertiesDictionary;
        [Export ("propertiesDictionary", ArgumentSemantic.Copy)]
        NSDictionary PropertiesDictionary { get; }
    }

    // @protocol SUVersionComparison
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface SUVersionComparison
    {
        // @required -(NSComparisonResult)compareVersion:(NSString *)versionA toVersion:(NSString *)versionB;
        [Abstract]
        [Export ("compareVersion:toVersion:")]
        NSComparisonResult CompareVersion (string versionA, string versionB);
    }

    // @interface SUStandardVersionComparator : NSObject <SUVersionComparison>
    [BaseType (typeof (NSObject))]
    interface SUStandardVersionComparator
    {
        // +(SUStandardVersionComparator *)defaultComparator;
        [Static]
        [Export ("defaultComparator")]
        SUStandardVersionComparator DefaultComparator { get; }

        // -(NSComparisonResult)compareVersion:(NSString *)versionA toVersion:(NSString *)versionB;
        [Export ("compareVersion:toVersion:")]
        NSComparisonResult CompareVersion (string versionA, string versionB);
    }

    // @protocol SUVersionDisplay
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface SUVersionDisplay
    {
        // @required -(void)formatVersion:(NSString **)inOutVersionA andVersion:(NSString **)inOutVersionB;
        [Abstract]
        [Export ("formatVersion:andVersion:")]
        void FormatVersion (out string inOutVersionA, out string inOutVersionB);
    }

    // @interface SUUpdater : NSObject
    [BaseType (typeof (NSObject))]
    interface SUUpdater
    {
        [Wrap ("WeakDelegate")]
        SUUpdaterDelegate Delegate { get; set; }

        // @property (unsafe_unretained) id<SUUpdaterDelegate> delegate __attribute__((iboutlet));
        [NullAllowed, Export ("delegate", ArgumentSemantic.Assign)]
        NSObject WeakDelegate { get; set; }

        // +(SUUpdater *)sharedUpdater;
        [Static]
        [Export ("sharedUpdater")]
        SUUpdater SharedUpdater { get; }

        // +(SUUpdater *)updaterForBundle:(NSBundle *)bundle;
        [Static]
        [Export ("updaterForBundle:")]
        SUUpdater UpdaterForBundle (NSBundle bundle);

        // -(instancetype)initForBundle:(NSBundle *)bundle;
        [Export ("initForBundle:")]
        IntPtr Constructor (NSBundle bundle);

        // @property (readonly, strong) NSBundle * hostBundle;
        [Export ("hostBundle", ArgumentSemantic.Strong)]
        NSBundle HostBundle { get; }

        // @property (readonly, strong) NSBundle * sparkleBundle;
        [Export ("sparkleBundle", ArgumentSemantic.Strong)]
        NSBundle SparkleBundle { get; }

        // @property BOOL automaticallyChecksForUpdates;
        [Export ("automaticallyChecksForUpdates")]
        bool AutomaticallyChecksForUpdates { get; set; }

        // @property NSTimeInterval updateCheckInterval;
        [Export ("updateCheckInterval")]
        double UpdateCheckInterval { get; set; }

        // @property (copy) NSURL * feedURL;
        [Export ("feedURL", ArgumentSemantic.Copy)]
        NSUrl FeedURL { get; set; }

        // @property (copy, nonatomic) NSString * userAgentString;
        [Export ("userAgentString")]
        string UserAgentString { get; set; }

        // @property (copy) NSDictionary * httpHeaders;
        [Export ("httpHeaders", ArgumentSemantic.Copy)]
        NSDictionary HttpHeaders { get; set; }

        // @property BOOL sendsSystemProfile;
        [Export ("sendsSystemProfile")]
        bool SendsSystemProfile { get; set; }

        // @property BOOL automaticallyDownloadsUpdates;
        [Export ("automaticallyDownloadsUpdates")]
        bool AutomaticallyDownloadsUpdates { get; set; }

        // @property (copy, nonatomic) NSString * decryptionPassword;
        [Export ("decryptionPassword")]
        string DecryptionPassword { get; set; }

        // -(void)checkForUpdates:(id)sender __attribute__((ibaction));
        [Export ("checkForUpdates:")]
        void CheckForUpdates (NSObject sender);

        // -(void)checkForUpdatesInBackground;
        [Export ("checkForUpdatesInBackground")]
        void CheckForUpdatesInBackground ();

        // -(void)installUpdatesIfAvailable;
        [Export ("installUpdatesIfAvailable")]
        void InstallUpdatesIfAvailable ();

        // @property (readonly, copy) NSDate * lastUpdateCheckDate;
        [Export ("lastUpdateCheckDate", ArgumentSemantic.Copy)]
        NSDate LastUpdateCheckDate { get; }

        // -(void)checkForUpdateInformation;
        [Export ("checkForUpdateInformation")]
        void CheckForUpdateInformation ();

        // -(void)resetUpdateCycle;
        [Export ("resetUpdateCycle")]
        void ResetUpdateCycle ();

        // @property (readonly) BOOL updateInProgress;
        [Export ("updateInProgress")]
        bool UpdateInProgress { get; }
    }

    [Static]
    partial interface Notifications
    {
        // extern NSString *const SUUpdaterDidFinishLoadingAppCastNotification;
        [Field ("SUUpdaterDidFinishLoadingAppCastNotification", LibraryName = "__Internal")]
        NSString SUUpdaterDidFinishLoadingAppCastNotification { get; }

        // extern NSString *const SUUpdaterDidFindValidUpdateNotification;
        [Field ("SUUpdaterDidFindValidUpdateNotification", LibraryName = "__Internal")]
        NSString SUUpdaterDidFindValidUpdateNotification { get; }

        // extern NSString *const SUUpdaterDidNotFindUpdateNotification;
        [Field ("SUUpdaterDidNotFindUpdateNotification", LibraryName = "__Internal")]
        NSString SUUpdaterDidNotFindUpdateNotification { get; }

        // extern NSString *const SUUpdaterWillRestartNotification;
        [Field ("SUUpdaterWillRestartNotification", LibraryName = "__Internal")]
        NSString SUUpdaterWillRestartNotification { get; }

        // extern NSString *const SUUpdaterAppcastItemNotificationKey;
        [Field ("SUUpdaterAppcastItemNotificationKey", LibraryName = "__Internal")]
        NSString SUUpdaterAppcastItemNotificationKey { get; }

        // extern NSString *const SUUpdaterAppcastNotificationKey;
        [Field ("SUUpdaterAppcastNotificationKey", LibraryName = "__Internal")]
        NSString SUUpdaterAppcastNotificationKey { get; }
    }


    // @protocol SUUpdaterDelegate <NSObject>
    [Protocol, Model]
    [BaseType (typeof (NSObject))]
    interface SUUpdaterDelegate
    {
        // @optional -(BOOL)updaterMayCheckForUpdates:(SUUpdater *)updater;
        [Export ("updaterMayCheckForUpdates:")]
        bool UpdaterMayCheckForUpdates (SUUpdater updater);

        // @optional -(NSArray *)feedParametersForUpdater:(SUUpdater *)updater sendingSystemProfile:(BOOL)sendingProfile;
        [Export ("feedParametersForUpdater:sendingSystemProfile:")]
        NSObject [] FeedParametersForUpdater (SUUpdater updater, bool sendingProfile);

        // @optional -(NSString *)feedURLStringForUpdater:(SUUpdater *)updater;
        [Export ("feedURLStringForUpdater:")]
        string FeedURLStringForUpdater (SUUpdater updater);

        // @optional -(BOOL)updaterShouldPromptForPermissionToCheckForUpdates:(SUUpdater *)updater;
        [Export ("updaterShouldPromptForPermissionToCheckForUpdates:")]
        bool UpdaterShouldPromptForPermissionToCheckForUpdates (SUUpdater updater);

        // @optional -(void)updater:(SUUpdater *)updater didFinishLoadingAppcast:(SUAppcast *)appcast;
        [Export ("updater:didFinishLoadingAppcast:")]
        void Updater (SUUpdater updater, SUAppcast appcast);

        // @optional -(SUAppcastItem *)bestValidUpdateInAppcast:(SUAppcast *)appcast forUpdater:(SUUpdater *)updater;
        [Export ("bestValidUpdateInAppcast:forUpdater:")]
        SUAppcastItem BestValidUpdateInAppcast (SUAppcast appcast, SUUpdater updater);

        // @optional -(void)updater:(SUUpdater *)updater didFindValidUpdate:(SUAppcastItem *)item;
        [Export ("updater:didFindValidUpdate:")]
        void Updater (SUUpdater updater, SUAppcastItem item);

        // @optional -(void)updaterDidNotFindUpdate:(SUUpdater *)updater;
        [Export ("updaterDidNotFindUpdate:")]
        void UpdaterDidNotFindUpdate (SUUpdater updater);

        // @optional -(void)updater:(SUUpdater *)updater willDownloadUpdate:(SUAppcastItem *)item withRequest:(NSMutableURLRequest *)request;
        [Export ("updater:willDownloadUpdate:withRequest:")]
        void Updater (SUUpdater updater, SUAppcastItem item, NSMutableUrlRequest request);

        // @optional -(void)updater:(SUUpdater *)updater failedToDownloadUpdate:(SUAppcastItem *)item error:(NSError *)error;
        [Export ("updater:failedToDownloadUpdate:error:")]
        void Updater (SUUpdater updater, SUAppcastItem item, NSError error);

        // @optional -(void)userDidCancelDownload:(SUUpdater *)updater;
        [Export ("userDidCancelDownload:")]
        void UserDidCancelDownload (SUUpdater updater);

        // @optional -(void)updater:(SUUpdater *)updater willInstallUpdate:(SUAppcastItem *)item;
        [Export ("updater:willInstallUpdate:")]
        void UpdaterWillInstallUpdate (SUUpdater updater, SUAppcastItem item);

        // @optional -(BOOL)updater:(SUUpdater *)updater shouldPostponeRelaunchForUpdate:(SUAppcastItem *)item untilInvoking:(NSInvocation *)invocation;
        [Export ("updater:shouldPostponeRelaunchForUpdate:untilInvoking:")]
        bool UpdaterShouldPostponeRelaunchForUpdateUntil (SUUpdater updater, SUAppcastItem item, NSInvocation invocation);

        // @optional -(BOOL)updaterShouldRelaunchApplication:(SUUpdater *)updater;
        [Export ("updaterShouldRelaunchApplication:")]
        bool UpdaterShouldRelaunchApplication (SUUpdater updater);

        // @optional -(void)updaterWillRelaunchApplication:(SUUpdater *)updater;
        [Export ("updaterWillRelaunchApplication:")]
        void UpdaterWillRelaunchApplication (SUUpdater updater);

        // @optional -(id<SUVersionComparison>)versionComparatorForUpdater:(SUUpdater *)updater;
        [Export ("versionComparatorForUpdater:")]
        SUVersionComparison VersionComparatorForUpdater (SUUpdater updater);

        // @optional -(id<SUVersionDisplay>)versionDisplayerForUpdater:(SUUpdater *)updater;
        [Export ("versionDisplayerForUpdater:")]
        SUVersionDisplay VersionDisplayerForUpdater (SUUpdater updater);

        // @optional -(NSString *)pathToRelaunchForUpdater:(SUUpdater *)updater;
        [Export ("pathToRelaunchForUpdater:")]
        string PathToRelaunchForUpdater (SUUpdater updater);

        // @optional -(void)updaterWillShowModalAlert:(SUUpdater *)updater;
        [Export ("updaterWillShowModalAlert:")]
        void UpdaterWillShowModalAlert (SUUpdater updater);

        // @optional -(void)updaterDidShowModalAlert:(SUUpdater *)updater;
        [Export ("updaterDidShowModalAlert:")]
        void UpdaterDidShowModalAlert (SUUpdater updater);

        // @optional -(void)updater:(SUUpdater *)updater willInstallUpdateOnQuit:(SUAppcastItem *)item immediateInstallationInvocation:(NSInvocation *)invocation;
        [Export ("updater:willInstallUpdateOnQuit:immediateInstallationInvocation:")]
        void UpdaterWillInstallUpdateOnQuit (SUUpdater updater, SUAppcastItem item, NSInvocation invocation);

        // @optional -(void)updater:(SUUpdater *)updater didCancelInstallUpdateOnQuit:(SUAppcastItem *)item;
        [Export ("updater:didCancelInstallUpdateOnQuit:")]
        void UpdaterDidCancelInstallUpdateOnQuit (SUUpdater updater, SUAppcastItem item);

        // @optional -(void)updater:(SUUpdater *)updater didAbortWithError:(NSError *)error;
        [Export ("updater:didAbortWithError:")]
        void UpdaterDidAbortWithError (SUUpdater updater, NSError error);
    }


    [Static]
    partial interface Constants
    {
        // extern NSString *const SUSparkleErrorDomain;
        [Field ("SUSparkleErrorDomain", LibraryName = "__Internal")]
        NSString SUSparkleErrorDomain { get; }

    }
}