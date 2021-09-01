using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Genius.Models.Response;
using Genius.Models.Song;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using YarinGeorge.Utilities;
using YarinGeorge.Utilities.Extensions;
using YarinGeorge.Utilities.Extensions.GeniusAPI;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace ParTboT.Commands
{
    #region Request

    public class YouTubeVideoRequest
    {
        public Context context { get; set; }
        public string videoId { get; set; }
    }

    public class Context
    {
        public Client client { get; set; }
    }

    public class Client
    {
        public string hl { get; set; }
        public string clientName { get; set; }
        public string clientVersion { get; set; }
        public Mainappwebinfo mainAppWebInfo { get; set; }
    }

    public class Mainappwebinfo
    {
        public string graftUrl { get; set; }
    }
    #endregion Request

    #region Response

    public record YouTubeResponse
    {
        [JsonProperty("responseContext", NullValueHandling = NullValueHandling.Ignore)]
        public ResponseContext ResponseContext { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("playabilityStatus", NullValueHandling = NullValueHandling.Ignore)]
        public PlayabilityStatus PlayabilityStatus { get; set; }

        [JsonProperty("streamingData", NullValueHandling = NullValueHandling.Ignore)]
        public StreamingData StreamingData { get; set; }

        [JsonProperty("playerAds", NullValueHandling = NullValueHandling.Ignore)]
        public List<PlayerAd> PlayerAds { get; set; }

        [JsonProperty("playbackTracking", NullValueHandling = NullValueHandling.Ignore)]
        public PlaybackTracking PlaybackTracking { get; set; }

        [JsonProperty("captions", NullValueHandling = NullValueHandling.Ignore)]
        public Captions Captions { get; set; }

        [JsonProperty("videoDetails", NullValueHandling = NullValueHandling.Ignore)]
        public VideoDetails VideoDetails { get; set; }

        [JsonProperty("annotations", NullValueHandling = NullValueHandling.Ignore)]
        public List<Annotation> Annotations { get; set; }

        [JsonProperty("playerConfig", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerConfig PlayerConfig { get; set; }

        [JsonProperty("storyboards", NullValueHandling = NullValueHandling.Ignore)]
        public Storyboards Storyboards { get; set; }

        [JsonProperty("microformat", NullValueHandling = NullValueHandling.Ignore)]
        public Microformat Microformat { get; set; }

        [JsonProperty("cards", NullValueHandling = NullValueHandling.Ignore)]
        public Cards Cards { get; set; }

        [JsonProperty("attestation", NullValueHandling = NullValueHandling.Ignore)]
        public Attestation Attestation { get; set; }

        [JsonProperty("messages", NullValueHandling = NullValueHandling.Ignore)]
        public List<Message> Messages { get; set; }

        [JsonProperty("endscreen", NullValueHandling = NullValueHandling.Ignore)]
        public Endscreen Endscreen { get; set; }

        [JsonProperty("adPlacements", NullValueHandling = NullValueHandling.Ignore)]
        public List<AdPlacement> AdPlacements { get; set; }

        [JsonProperty("frameworkUpdates", NullValueHandling = NullValueHandling.Ignore)]
        public FrameworkUpdates FrameworkUpdates { get; set; }
    }

    public record AdPlacement
    {
        [JsonProperty("adPlacementRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public AdPlacementRenderer AdPlacementRenderer { get; set; }
    }

    public record AdPlacementRenderer
    {
        [JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
        public Config Config { get; set; }

        [JsonProperty("renderer", NullValueHandling = NullValueHandling.Ignore)]
        public Renderer Renderer { get; set; }

        [JsonProperty("adSlotLoggingData", NullValueHandling = NullValueHandling.Ignore)]
        public AdSlotLoggingData AdSlotLoggingData { get; set; }
    }

    public record AdSlotLoggingData
    {
        [JsonProperty("serializedSlotAdServingDataEntry", NullValueHandling = NullValueHandling.Ignore)]
        public string SerializedSlotAdServingDataEntry { get; set; }
    }

    public record Config
    {
        [JsonProperty("adPlacementConfig", NullValueHandling = NullValueHandling.Ignore)]
        public AdPlacementConfig AdPlacementConfig { get; set; }
    }

    public record AdPlacementConfig
    {
        [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
        public string Kind { get; set; }

        [JsonProperty("adTimeOffset", NullValueHandling = NullValueHandling.Ignore)]
        public AdTimeOffset AdTimeOffset { get; set; }

        [JsonProperty("hideCueRangeMarker", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HideCueRangeMarker { get; set; }
    }

    public record AdTimeOffset
    {
        [JsonProperty("offsetStartMilliseconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? OffsetStartMilliseconds { get; set; }

        [JsonProperty("offsetEndMilliseconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? OffsetEndMilliseconds { get; set; }
    }

    public record Renderer
    {
        [JsonProperty("adBreakServiceRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public AdBreakServiceRenderer AdBreakServiceRenderer { get; set; }
    }

    public record AdBreakServiceRenderer
    {
        [JsonProperty("prefetchMilliseconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? PrefetchMilliseconds { get; set; }

        [JsonProperty("getAdBreakUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri GetAdBreakUrl { get; set; }
    }

    public record Annotation
    {
        [JsonProperty("playerAnnotationsExpandedRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerAnnotationsExpandedRenderer PlayerAnnotationsExpandedRenderer { get; set; }
    }

    public record PlayerAnnotationsExpandedRenderer
    {
        [JsonProperty("featuredChannel", NullValueHandling = NullValueHandling.Ignore)]
        public FeaturedChannel FeaturedChannel { get; set; }

        [JsonProperty("allowSwipeDismiss", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowSwipeDismiss { get; set; }

        [JsonProperty("annotationId", NullValueHandling = NullValueHandling.Ignore)]
        public string? AnnotationId { get; set; }
    }

    public record FeaturedChannel
    {
        [JsonProperty("startTimeMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? StartTimeMs { get; set; }

        [JsonProperty("endTimeMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? EndTimeMs { get; set; }

        [JsonProperty("watermark", NullValueHandling = NullValueHandling.Ignore)]
        public WatermarkClass Watermark { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("navigationEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public FeaturedChannelNavigationEndpoint NavigationEndpoint { get; set; }

        [JsonProperty("channelName", NullValueHandling = NullValueHandling.Ignore)]
        public string ChannelName { get; set; }

        [JsonProperty("subscribeButton", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeButton SubscribeButton { get; set; }
    }

    public record FeaturedChannelNavigationEndpoint
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public NavigationEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("browseEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public EndpointBrowseEndpoint BrowseEndpoint { get; set; }
    }

    public record EndpointBrowseEndpoint
    {
        [JsonProperty("browseId", NullValueHandling = NullValueHandling.Ignore)]
        public string BrowseId { get; set; }
    }

    public record NavigationEndpointCommandMetadata
    {
        [JsonProperty("webCommandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleWebCommandMetadata WebCommandMetadata { get; set; }
    }

    public record PurpleWebCommandMetadata
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("webPageType", NullValueHandling = NullValueHandling.Ignore)]
        public string WebPageType { get; set; }

        [JsonProperty("rootVe", NullValueHandling = NullValueHandling.Ignore)]
        public long? RootVe { get; set; }

        [JsonProperty("apiUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string ApiUrl { get; set; }
    }

    public record SubscribeButton
    {
        [JsonProperty("subscribeButtonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeButtonSubscribeButtonRenderer SubscribeButtonRenderer { get; set; }
    }

    public record SubscribeButtonSubscribeButtonRenderer
    {
        [JsonProperty("buttonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle ButtonText { get; set; }

        [JsonProperty("subscribed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Subscribed { get; set; }

        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("channelId", NullValueHandling = NullValueHandling.Ignore)]
        public string ChannelId { get; set; }

        [JsonProperty("showPreferences", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowPreferences { get; set; }

        [JsonProperty("subscribedButtonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle SubscribedButtonText { get; set; }

        [JsonProperty("unsubscribedButtonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle UnsubscribedButtonText { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("unsubscribeButtonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle UnsubscribeButtonText { get; set; }

        [JsonProperty("serviceEndpoints", NullValueHandling = NullValueHandling.Ignore)]
        public List<PurpleServiceEndpoint> ServiceEndpoints { get; set; }

        [JsonProperty("subscribeAccessibility", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeAccessibilityClass SubscribeAccessibility { get; set; }

        [JsonProperty("unsubscribeAccessibility", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeAccessibilityClass UnsubscribeAccessibility { get; set; }

        [JsonProperty("signInEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public SignInEndpoint SignInEndpoint { get; set; }
    }

    public record MessageTitle
    {
        [JsonProperty("runs", NullValueHandling = NullValueHandling.Ignore)]
        public List<Run> Runs { get; set; }
    }

    public record Run
    {
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
    }

    public record PurpleServiceEndpoint
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("subscribeEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeEndpoint SubscribeEndpoint { get; set; }

        [JsonProperty("signalServiceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleSignalServiceEndpoint SignalServiceEndpoint { get; set; }
    }

    public record ServiceEndpointCommandMetadata
    {
        [JsonProperty("webCommandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyWebCommandMetadata WebCommandMetadata { get; set; }
    }

    public record FluffyWebCommandMetadata
    {
        [JsonProperty("sendPost", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SendPost { get; set; }

        [JsonProperty("apiUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string ApiUrl { get; set; }
    }

    public record PurpleSignalServiceEndpoint
    {
        [JsonProperty("signal", NullValueHandling = NullValueHandling.Ignore)]
        public string Signal { get; set; }

        [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
        public List<PurpleAction> Actions { get; set; }
    }

    public record PurpleAction
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("openPopupAction", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleOpenPopupAction OpenPopupAction { get; set; }
    }

    public record PurpleOpenPopupAction
    {
        [JsonProperty("popup", NullValueHandling = NullValueHandling.Ignore)]
        public PurplePopup Popup { get; set; }

        [JsonProperty("popupType", NullValueHandling = NullValueHandling.Ignore)]
        public string PopupType { get; set; }
    }

    public record PurplePopup
    {
        [JsonProperty("confirmDialogRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleConfirmDialogRenderer ConfirmDialogRenderer { get; set; }
    }

    public record PurpleConfirmDialogRenderer
    {
        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("dialogMessages", NullValueHandling = NullValueHandling.Ignore)]
        public List<MessageTitle> DialogMessages { get; set; }

        [JsonProperty("confirmButton", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleConfirmButton ConfirmButton { get; set; }

        [JsonProperty("cancelButton", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleCancelButton CancelButton { get; set; }

        [JsonProperty("primaryIsCancel", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrimaryIsCancel { get; set; }
    }

    public record PurpleCancelButton
    {
        [JsonProperty("buttonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleButtonRenderer ButtonRenderer { get; set; }
    }

    public record PurpleButtonRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("isDisabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDisabled { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle Text { get; set; }

        [JsonProperty("accessibility", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilityDataClass Accessibility { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record AccessibilityDataClass
    {
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }
    }

    public record PurpleConfirmButton
    {
        [JsonProperty("buttonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyButtonRenderer ButtonRenderer { get; set; }
    }

    public record FluffyButtonRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("isDisabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDisabled { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle Text { get; set; }

        [JsonProperty("serviceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public UnsubscribeCommand ServiceEndpoint { get; set; }

        [JsonProperty("accessibility", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilityDataClass Accessibility { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record UnsubscribeCommand
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("unsubscribeEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeEndpoint UnsubscribeEndpoint { get; set; }
    }

    public record SubscribeEndpoint
    {
        [JsonProperty("channelIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ChannelIds { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public string Params { get; set; }
    }

    public record SignInEndpoint
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public SignInEndpointCommandMetadata CommandMetadata { get; set; }
    }

    public record SignInEndpointCommandMetadata
    {
        [JsonProperty("webCommandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public WebCommandMetadata WebCommandMetadata { get; set; }
    }

    public record WebCommandMetadata
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }

    public record SubscribeAccessibilityClass
    {
        [JsonProperty("accessibilityData", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilityDataClass AccessibilityData { get; set; }
    }

    public record WatermarkClass
    {
        [JsonProperty("thumbnails", NullValueHandling = NullValueHandling.Ignore)]
        public List<ThumbnailThumbnail> Thumbnails { get; set; }
    }

    public record ThumbnailThumbnail
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }
    }

    public record Attestation
    {
        [JsonProperty("playerAttestationRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerAttestationRenderer PlayerAttestationRenderer { get; set; }
    }

    public record PlayerAttestationRenderer
    {
        [JsonProperty("challenge", NullValueHandling = NullValueHandling.Ignore)]
        public string Challenge { get; set; }

        [JsonProperty("botguardData", NullValueHandling = NullValueHandling.Ignore)]
        public BotguardData BotguardData { get; set; }
    }

    public record BotguardData
    {
        [JsonProperty("program", NullValueHandling = NullValueHandling.Ignore)]
        public string Program { get; set; }

        [JsonProperty("interpreterUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string InterpreterUrl { get; set; }

        [JsonProperty("interpreterSafeUrl", NullValueHandling = NullValueHandling.Ignore)]
        public InterpreterSafeUrl InterpreterSafeUrl { get; set; }
    }

    public record InterpreterSafeUrl
    {
        [JsonProperty("privateDoNotAccessOrElseTrustedResourceUrlWrappedValue", NullValueHandling = NullValueHandling.Ignore)]
        public string PrivateDoNotAccessOrElseTrustedResourceUrlWrappedValue { get; set; }
    }

    public record Captions
    {
        [JsonProperty("playerCaptionsRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerCaptionsRenderer PlayerCaptionsRenderer { get; set; }

        [JsonProperty("playerCaptionsTracklistRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerCaptionsTracklistRenderer PlayerCaptionsTracklistRenderer { get; set; }
    }

    public record PlayerCaptionsRenderer
    {
        [JsonProperty("baseUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri BaseUrl { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public string Visibility { get; set; }
    }

    public record PlayerCaptionsTracklistRenderer
    {
        [JsonProperty("captionTracks", NullValueHandling = NullValueHandling.Ignore)]
        public List<CaptionTrack> CaptionTracks { get; set; }

        [JsonProperty("audioTracks", NullValueHandling = NullValueHandling.Ignore)]
        public List<AudioTrack> AudioTracks { get; set; }

        [JsonProperty("translationLanguages", NullValueHandling = NullValueHandling.Ignore)]
        public List<TranslationLanguage> TranslationLanguages { get; set; }

        [JsonProperty("defaultAudioTrackIndex", NullValueHandling = NullValueHandling.Ignore)]
        public long? DefaultAudioTrackIndex { get; set; }
    }

    public record AudioTrack
    {
        [JsonProperty("captionTrackIndices", NullValueHandling = NullValueHandling.Ignore)]
        public List<long> CaptionTrackIndices { get; set; }
    }

    public record CaptionTrack
    {
        [JsonProperty("baseUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri BaseUrl { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText Name { get; set; }

        [JsonProperty("vssId", NullValueHandling = NullValueHandling.Ignore)]
        public string VssId { get; set; }

        [JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
        public string LanguageCode { get; set; }

        [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
        public string Kind { get; set; }

        [JsonProperty("isTranslatable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTranslatable { get; set; }
    }

    public record HeaderText
    {
        [JsonProperty("simpleText", NullValueHandling = NullValueHandling.Ignore)]
        public string SimpleText { get; set; }
    }

    public record TranslationLanguage
    {
        [JsonProperty("languageCode", NullValueHandling = NullValueHandling.Ignore)]
        public string LanguageCode { get; set; }

        [JsonProperty("languageName", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText LanguageName { get; set; }
    }

    public record Cards
    {
        [JsonProperty("cardCollectionRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public CardCollectionRenderer CardCollectionRenderer { get; set; }
    }

    public record CardCollectionRenderer
    {
        [JsonProperty("cards", NullValueHandling = NullValueHandling.Ignore)]
        public List<Card> Cards { get; set; }

        [JsonProperty("headerText", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText HeaderText { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public CloseButton Icon { get; set; }

        [JsonProperty("closeButton", NullValueHandling = NullValueHandling.Ignore)]
        public CloseButton CloseButton { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("allowTeaserDismiss", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowTeaserDismiss { get; set; }

        [JsonProperty("logIconVisibilityUpdates", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LogIconVisibilityUpdates { get; set; }
    }

    public record Card
    {
        [JsonProperty("cardRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public CardRenderer CardRenderer { get; set; }
    }

    public record CardRenderer
    {
        [JsonProperty("teaser", NullValueHandling = NullValueHandling.Ignore)]
        public Teaser Teaser { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public Content Content { get; set; }

        [JsonProperty("cueRanges", NullValueHandling = NullValueHandling.Ignore)]
        public List<CueRange> CueRanges { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public CloseButton Icon { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("cardId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? CardId { get; set; }

        [JsonProperty("feature", NullValueHandling = NullValueHandling.Ignore)]
        public string Feature { get; set; }
    }

    public record Content
    {
        [JsonProperty("playlistInfoCardContentRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlaylistInfoCardContentRenderer PlaylistInfoCardContentRenderer { get; set; }
    }

    public record PlaylistInfoCardContentRenderer
    {
        [JsonProperty("playlistThumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public WatermarkClass PlaylistThumbnail { get; set; }

        [JsonProperty("playlistVideoCount", NullValueHandling = NullValueHandling.Ignore)]
        public PlaylistVideoCount PlaylistVideoCount { get; set; }

        [JsonProperty("playlistTitle", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText PlaylistTitle { get; set; }

        [JsonProperty("channelName", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText ChannelName { get; set; }

        [JsonProperty("videoCountText", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText VideoCountText { get; set; }

        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public PlaylistInfoCardContentRendererAction Action { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record PlaylistInfoCardContentRendererAction
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ActionCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("watchEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public ActionWatchEndpoint WatchEndpoint { get; set; }
    }

    public record ActionCommandMetadata
    {
        [JsonProperty("webCommandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public TentacledWebCommandMetadata WebCommandMetadata { get; set; }
    }

    public record TentacledWebCommandMetadata
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("webPageType", NullValueHandling = NullValueHandling.Ignore)]
        public string WebPageType { get; set; }

        [JsonProperty("rootVe", NullValueHandling = NullValueHandling.Ignore)]
        public long? RootVe { get; set; }
    }

    public record ActionWatchEndpoint
    {
        [JsonProperty("videoId", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoId { get; set; }

        [JsonProperty("playlistId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlaylistId { get; set; }

        [JsonProperty("watchEndpointSupportedOnesieConfig", NullValueHandling = NullValueHandling.Ignore)]
        public WatchEndpointSupportedOnesieConfig WatchEndpointSupportedOnesieConfig { get; set; }
    }

    public record WatchEndpointSupportedOnesieConfig
    {
        [JsonProperty("html5PlaybackOnesieConfig", NullValueHandling = NullValueHandling.Ignore)]
        public Html5PlaybackOnesieConfig Html5PlaybackOnesieConfig { get; set; }
    }

    public record Html5PlaybackOnesieConfig
    {
        [JsonProperty("commonConfig", NullValueHandling = NullValueHandling.Ignore)]
        public WebCommandMetadata CommonConfig { get; set; }
    }

    public record PlaylistVideoCount
    {
        [JsonProperty("simpleText", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? SimpleText { get; set; }
    }

    public record CueRange
    {
        [JsonProperty("startCardActiveMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? StartCardActiveMs { get; set; }

        [JsonProperty("endCardActiveMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? EndCardActiveMs { get; set; }

        [JsonProperty("teaserDurationMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? TeaserDurationMs { get; set; }

        [JsonProperty("iconAfterTeaserMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? IconAfterTeaserMs { get; set; }
    }

    public record CloseButton
    {
        [JsonProperty("infoCardIconRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public InfoCardIconRenderer InfoCardIconRenderer { get; set; }
    }

    public record InfoCardIconRenderer
    {
        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record Teaser
    {
        [JsonProperty("simpleCardTeaserRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public SimpleCardTeaserRenderer SimpleCardTeaserRenderer { get; set; }
    }

    public record SimpleCardTeaserRenderer
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText Message { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("prominent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Prominent { get; set; }

        [JsonProperty("logVisibilityUpdates", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LogVisibilityUpdates { get; set; }
    }

    public record Endscreen
    {
        [JsonProperty("endscreenRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public EndscreenRenderer EndscreenRenderer { get; set; }
    }

    public record EndscreenRenderer
    {
        [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
        public List<Element> Elements { get; set; }

        [JsonProperty("startMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? StartMs { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record Element
    {
        [JsonProperty("endscreenElementRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public EndscreenElementRenderer EndscreenElementRenderer { get; set; }
    }

    public record EndscreenElementRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public WatermarkClass Image { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public EndscreenElementRendererIcon Icon { get; set; }

        [JsonProperty("left", NullValueHandling = NullValueHandling.Ignore)]
        public double? Left { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public double? Width { get; set; }

        [JsonProperty("top", NullValueHandling = NullValueHandling.Ignore)]
        public double? Top { get; set; }

        [JsonProperty("aspectRatio", NullValueHandling = NullValueHandling.Ignore)]
        public double? AspectRatio { get; set; }

        [JsonProperty("startMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? StartMs { get; set; }

        [JsonProperty("endMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? EndMs { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public Title Title { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText Metadata { get; set; }

        [JsonProperty("callToAction", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText CallToAction { get; set; }

        [JsonProperty("dismiss", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText Dismiss { get; set; }

        [JsonProperty("endpoint", NullValueHandling = NullValueHandling.Ignore)]
        public Endpoint Endpoint { get; set; }

        [JsonProperty("hovercardButton", NullValueHandling = NullValueHandling.Ignore)]
        public HovercardButton HovercardButton { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("isSubscribe", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsSubscribe { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("subscribersText", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText SubscribersText { get; set; }

        [JsonProperty("videoDuration", NullValueHandling = NullValueHandling.Ignore)]
        public Title VideoDuration { get; set; }
    }

    public record Endpoint
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public NavigationEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("browseEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public EndpointBrowseEndpoint BrowseEndpoint { get; set; }

        [JsonProperty("watchEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public EndpointWatchEndpoint WatchEndpoint { get; set; }
    }

    public record EndpointWatchEndpoint
    {
        [JsonProperty("videoId", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoId { get; set; }

        [JsonProperty("watchEndpointSupportedOnesieConfig", NullValueHandling = NullValueHandling.Ignore)]
        public WatchEndpointSupportedOnesieConfig WatchEndpointSupportedOnesieConfig { get; set; }
    }

    public record HovercardButton
    {
        [JsonProperty("subscribeButtonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public HovercardButtonSubscribeButtonRenderer SubscribeButtonRenderer { get; set; }
    }

    public record HovercardButtonSubscribeButtonRenderer
    {
        [JsonProperty("buttonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle ButtonText { get; set; }

        [JsonProperty("subscribed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Subscribed { get; set; }

        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("channelId", NullValueHandling = NullValueHandling.Ignore)]
        public string ChannelId { get; set; }

        [JsonProperty("showPreferences", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowPreferences { get; set; }

        [JsonProperty("subscribedButtonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle SubscribedButtonText { get; set; }

        [JsonProperty("unsubscribedButtonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle UnsubscribedButtonText { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("unsubscribeButtonText", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle UnsubscribeButtonText { get; set; }

        [JsonProperty("serviceEndpoints", NullValueHandling = NullValueHandling.Ignore)]
        public List<FluffyServiceEndpoint> ServiceEndpoints { get; set; }

        [JsonProperty("subscribeAccessibility", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeAccessibilityClass SubscribeAccessibility { get; set; }

        [JsonProperty("unsubscribeAccessibility", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeAccessibilityClass UnsubscribeAccessibility { get; set; }

        [JsonProperty("signInEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public SignInEndpoint SignInEndpoint { get; set; }
    }

    public record FluffyServiceEndpoint
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("subscribeEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeEndpoint SubscribeEndpoint { get; set; }

        [JsonProperty("signalServiceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public FluffySignalServiceEndpoint SignalServiceEndpoint { get; set; }
    }

    public record FluffySignalServiceEndpoint
    {
        [JsonProperty("signal", NullValueHandling = NullValueHandling.Ignore)]
        public string Signal { get; set; }

        [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
        public List<FluffyAction> Actions { get; set; }
    }

    public record FluffyAction
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("openPopupAction", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyOpenPopupAction OpenPopupAction { get; set; }
    }

    public record FluffyOpenPopupAction
    {
        [JsonProperty("popup", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyPopup Popup { get; set; }

        [JsonProperty("popupType", NullValueHandling = NullValueHandling.Ignore)]
        public string PopupType { get; set; }
    }

    public record FluffyPopup
    {
        [JsonProperty("confirmDialogRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyConfirmDialogRenderer ConfirmDialogRenderer { get; set; }
    }

    public record FluffyConfirmDialogRenderer
    {
        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("dialogMessages", NullValueHandling = NullValueHandling.Ignore)]
        public List<MessageTitle> DialogMessages { get; set; }

        [JsonProperty("confirmButton", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyConfirmButton ConfirmButton { get; set; }

        [JsonProperty("cancelButton", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyCancelButton CancelButton { get; set; }

        [JsonProperty("primaryIsCancel", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PrimaryIsCancel { get; set; }
    }

    public record FluffyCancelButton
    {
        [JsonProperty("buttonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public TentacledButtonRenderer ButtonRenderer { get; set; }
    }

    public record TentacledButtonRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("isDisabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDisabled { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle Text { get; set; }

        [JsonProperty("accessibility", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilityDataClass Accessibility { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record FluffyConfirmButton
    {
        [JsonProperty("buttonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public StickyButtonRenderer ButtonRenderer { get; set; }
    }

    public record StickyButtonRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("isDisabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDisabled { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle Text { get; set; }

        [JsonProperty("serviceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public UnsubscribeCommand ServiceEndpoint { get; set; }

        [JsonProperty("accessibility", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilityDataClass Accessibility { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record EndscreenElementRendererIcon
    {
        [JsonProperty("thumbnails", NullValueHandling = NullValueHandling.Ignore)]
        public List<PurpleThumbnail> Thumbnails { get; set; }
    }

    public record PurpleThumbnail
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }

    public record Title
    {
        [JsonProperty("accessibility", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeAccessibilityClass Accessibility { get; set; }

        [JsonProperty("simpleText", NullValueHandling = NullValueHandling.Ignore)]
        public string SimpleText { get; set; }
    }

    public record FrameworkUpdates
    {
        [JsonProperty("entityBatchUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public EntityBatchUpdate EntityBatchUpdate { get; set; }
    }

    public record EntityBatchUpdate
    {
        [JsonProperty("mutations", NullValueHandling = NullValueHandling.Ignore)]
        public List<Mutation> Mutations { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public Timestamp Timestamp { get; set; }
    }

    public record Mutation
    {
        [JsonProperty("entityKey", NullValueHandling = NullValueHandling.Ignore)]
        public string EntityKey { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("payload", NullValueHandling = NullValueHandling.Ignore)]
        public Payload Payload { get; set; }
    }

    public record Payload
    {
        [JsonProperty("offlineabilityEntity", NullValueHandling = NullValueHandling.Ignore)]
        public OfflineabilityEntity OfflineabilityEntity { get; set; }
    }

    public record OfflineabilityEntity
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("accessState", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessState { get; set; }
    }

    public record Timestamp
    {
        [JsonProperty("seconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? Seconds { get; set; }

        [JsonProperty("nanos", NullValueHandling = NullValueHandling.Ignore)]
        public long? Nanos { get; set; }
    }

    public record Message
    {
        [JsonProperty("mealbarPromoRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public MealbarPromoRenderer MealbarPromoRenderer { get; set; }
    }

    public record MealbarPromoRenderer
    {
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public MealbarPromoRendererIcon Icon { get; set; }

        [JsonProperty("messageTexts", NullValueHandling = NullValueHandling.Ignore)]
        public List<MessageTitle> MessageTexts { get; set; }

        [JsonProperty("actionButton", NullValueHandling = NullValueHandling.Ignore)]
        public ActionButton ActionButton { get; set; }

        [JsonProperty("dismissButton", NullValueHandling = NullValueHandling.Ignore)]
        public DismissButton DismissButton { get; set; }

        [JsonProperty("triggerCondition", NullValueHandling = NullValueHandling.Ignore)]
        public string TriggerCondition { get; set; }

        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }

        [JsonProperty("impressionEndpoints", NullValueHandling = NullValueHandling.Ignore)]
        public List<ImpressionEndpointElement> ImpressionEndpoints { get; set; }

        [JsonProperty("isVisible", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsVisible { get; set; }

        [JsonProperty("messageTitle", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle MessageTitle { get; set; }
    }

    public record ActionButton
    {
        [JsonProperty("buttonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public ActionButtonButtonRenderer ButtonRenderer { get; set; }
    }

    public record ActionButtonButtonRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle Text { get; set; }

        [JsonProperty("serviceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public ImpressionEndpointElement ServiceEndpoint { get; set; }

        [JsonProperty("navigationEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public ButtonRendererNavigationEndpoint NavigationEndpoint { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record ButtonRendererNavigationEndpoint
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public NavigationEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("browseEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleBrowseEndpoint BrowseEndpoint { get; set; }
    }

    public record PurpleBrowseEndpoint
    {
        [JsonProperty("browseId", NullValueHandling = NullValueHandling.Ignore)]
        public string BrowseId { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public string Params { get; set; }
    }

    public record ImpressionEndpointElement
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("feedbackEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public FeedbackEndpoint FeedbackEndpoint { get; set; }
    }

    public record FeedbackEndpoint
    {
        [JsonProperty("feedbackToken", NullValueHandling = NullValueHandling.Ignore)]
        public string FeedbackToken { get; set; }

        [JsonProperty("uiActions", NullValueHandling = NullValueHandling.Ignore)]
        public UiActions UiActions { get; set; }
    }

    public record UiActions
    {
        [JsonProperty("hideEnclosingContainer", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HideEnclosingContainer { get; set; }
    }

    public record DismissButton
    {
        [JsonProperty("buttonRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public DismissButtonButtonRenderer ButtonRenderer { get; set; }
    }

    public record DismissButtonButtonRenderer
    {
        [JsonProperty("style", NullValueHandling = NullValueHandling.Ignore)]
        public string Style { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public string Size { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public MessageTitle Text { get; set; }

        [JsonProperty("serviceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public ImpressionEndpointElement ServiceEndpoint { get; set; }

        [JsonProperty("trackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string TrackingParams { get; set; }
    }

    public record MealbarPromoRendererIcon
    {
        [JsonProperty("thumbnails", NullValueHandling = NullValueHandling.Ignore)]
        public List<FluffyThumbnail> Thumbnails { get; set; }
    }

    public record FluffyThumbnail
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }
    }

    public record Microformat
    {
        [JsonProperty("playerMicroformatRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerMicroformatRenderer PlayerMicroformatRenderer { get; set; }
    }

    public record PlayerMicroformatRenderer
    {
        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public WatermarkClass Thumbnail { get; set; }

        [JsonProperty("embed", NullValueHandling = NullValueHandling.Ignore)]
        public Embed Embed { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public HeaderText Description { get; set; }

        [JsonProperty("lengthSeconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? LengthSeconds { get; set; }

        [JsonProperty("ownerProfileUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri OwnerProfileUrl { get; set; }

        [JsonProperty("externalChannelId", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalChannelId { get; set; }

        [JsonProperty("isFamilySafe", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFamilySafe { get; set; }

        [JsonProperty("availableCountries", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AvailableCountries { get; set; }

        [JsonProperty("isUnlisted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsUnlisted { get; set; }

        [JsonProperty("hasYpcMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasYpcMetadata { get; set; }

        [JsonProperty("viewCount", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ViewCount { get; set; }

        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("publishDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? PublishDate { get; set; }

        [JsonProperty("ownerChannelName", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerChannelName { get; set; }

        [JsonProperty("uploadDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? UploadDate { get; set; }
    }

    public record Embed
    {
        [JsonProperty("iframeUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri IframeUrl { get; set; }

        [JsonProperty("flashUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri FlashUrl { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("flashSecureUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri FlashSecureUrl { get; set; }
    }

    public record PlayabilityStatus
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("playableInEmbed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PlayableInEmbed { get; set; }

        [JsonProperty("miniplayer", NullValueHandling = NullValueHandling.Ignore)]
        public Miniplayer Miniplayer { get; set; }

        [JsonProperty("contextParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ContextParams { get; set; }
    }

    public record Miniplayer
    {
        [JsonProperty("miniplayerRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public MiniplayerRenderer MiniplayerRenderer { get; set; }
    }

    public record MiniplayerRenderer
    {
        [JsonProperty("playbackMode", NullValueHandling = NullValueHandling.Ignore)]
        public string PlaybackMode { get; set; }
    }

    public record PlaybackTracking
    {
        [JsonProperty("videostatsPlaybackUrl", NullValueHandling = NullValueHandling.Ignore)]
        public PtrackingUrlClass VideostatsPlaybackUrl { get; set; }

        [JsonProperty("videostatsDelayplayUrl", NullValueHandling = NullValueHandling.Ignore)]
        public PtrackingUrlClass VideostatsDelayplayUrl { get; set; }

        [JsonProperty("videostatsWatchtimeUrl", NullValueHandling = NullValueHandling.Ignore)]
        public PtrackingUrlClass VideostatsWatchtimeUrl { get; set; }

        [JsonProperty("ptrackingUrl", NullValueHandling = NullValueHandling.Ignore)]
        public PtrackingUrlClass PtrackingUrl { get; set; }

        [JsonProperty("qoeUrl", NullValueHandling = NullValueHandling.Ignore)]
        public PtrackingUrlClass QoeUrl { get; set; }

        [JsonProperty("atrUrl", NullValueHandling = NullValueHandling.Ignore)]
        public AtrUrlClass AtrUrl { get; set; }

        [JsonProperty("videostatsScheduledFlushWalltimeSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public List<long> VideostatsScheduledFlushWalltimeSeconds { get; set; }

        [JsonProperty("videostatsDefaultFlushIntervalSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? VideostatsDefaultFlushIntervalSeconds { get; set; }

        [JsonProperty("youtubeRemarketingUrl", NullValueHandling = NullValueHandling.Ignore)]
        public AtrUrlClass YoutubeRemarketingUrl { get; set; }
    }

    public record AtrUrlClass
    {
        [JsonProperty("baseUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri BaseUrl { get; set; }

        [JsonProperty("elapsedMediaTimeSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? ElapsedMediaTimeSeconds { get; set; }
    }

    public record PtrackingUrlClass
    {
        [JsonProperty("baseUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri BaseUrl { get; set; }
    }

    public record PlayerAd
    {
        [JsonProperty("playerLegacyDesktopWatchAdsRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerLegacyDesktopWatchAdsRenderer PlayerLegacyDesktopWatchAdsRenderer { get; set; }
    }

    public record PlayerLegacyDesktopWatchAdsRenderer
    {
        [JsonProperty("playerAdParams", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerAdParams PlayerAdParams { get; set; }

        [JsonProperty("gutParams", NullValueHandling = NullValueHandling.Ignore)]
        public GutParams GutParams { get; set; }

        [JsonProperty("showCompanion", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowCompanion { get; set; }

        [JsonProperty("showInstream", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowInstream { get; set; }

        [JsonProperty("useGut", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseGut { get; set; }
    }

    public record GutParams
    {
        [JsonProperty("tag", NullValueHandling = NullValueHandling.Ignore)]
        public string Tag { get; set; }
    }

    public record PlayerAdParams
    {
        [JsonProperty("showContentThumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowContentThumbnail { get; set; }

        [JsonProperty("enabledEngageTypes", NullValueHandling = NullValueHandling.Ignore)]
        public string EnabledEngageTypes { get; set; }
    }

    public record PlayerConfig
    {
        [JsonProperty("audioConfig", NullValueHandling = NullValueHandling.Ignore)]
        public AudioConfig AudioConfig { get; set; }

        [JsonProperty("streamSelectionConfig", NullValueHandling = NullValueHandling.Ignore)]
        public StreamSelectionConfig StreamSelectionConfig { get; set; }

        [JsonProperty("mediaCommonConfig", NullValueHandling = NullValueHandling.Ignore)]
        public MediaCommonConfig MediaCommonConfig { get; set; }

        [JsonProperty("webPlayerConfig", NullValueHandling = NullValueHandling.Ignore)]
        public WebPlayerConfig WebPlayerConfig { get; set; }
    }

    public record AudioConfig
    {
        [JsonProperty("loudnessDb", NullValueHandling = NullValueHandling.Ignore)]
        public double? LoudnessDb { get; set; }

        [JsonProperty("perceptualLoudnessDb", NullValueHandling = NullValueHandling.Ignore)]
        public double? PerceptualLoudnessDb { get; set; }

        [JsonProperty("enablePerFormatLoudness", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnablePerFormatLoudness { get; set; }
    }

    public record MediaCommonConfig
    {
        [JsonProperty("dynamicReadaheadConfig", NullValueHandling = NullValueHandling.Ignore)]
        public DynamicReadaheadConfig DynamicReadaheadConfig { get; set; }
    }

    public record DynamicReadaheadConfig
    {
        [JsonProperty("maxReadAheadMediaTimeMs", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxReadAheadMediaTimeMs { get; set; }

        [JsonProperty("minReadAheadMediaTimeMs", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinReadAheadMediaTimeMs { get; set; }

        [JsonProperty("readAheadGrowthRateMs", NullValueHandling = NullValueHandling.Ignore)]
        public long? ReadAheadGrowthRateMs { get; set; }
    }

    public record StreamSelectionConfig
    {
        [JsonProperty("maxBitrate", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? MaxBitrate { get; set; }
    }

    public record WebPlayerConfig
    {
        [JsonProperty("webPlayerActionsPorting", NullValueHandling = NullValueHandling.Ignore)]
        public WebPlayerActionsPorting WebPlayerActionsPorting { get; set; }
    }

    public record WebPlayerActionsPorting
    {
        [JsonProperty("getSharePanelCommand", NullValueHandling = NullValueHandling.Ignore)]
        public GetSharePanelCommand GetSharePanelCommand { get; set; }

        [JsonProperty("subscribeCommand", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeCommand SubscribeCommand { get; set; }

        [JsonProperty("unsubscribeCommand", NullValueHandling = NullValueHandling.Ignore)]
        public UnsubscribeCommand UnsubscribeCommand { get; set; }

        [JsonProperty("addToWatchLaterCommand", NullValueHandling = NullValueHandling.Ignore)]
        public AddToWatchLaterCommand AddToWatchLaterCommand { get; set; }

        [JsonProperty("removeFromWatchLaterCommand", NullValueHandling = NullValueHandling.Ignore)]
        public RemoveFromWatchLaterCommand RemoveFromWatchLaterCommand { get; set; }
    }

    public record AddToWatchLaterCommand
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("playlistEditEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public AddToWatchLaterCommandPlaylistEditEndpoint PlaylistEditEndpoint { get; set; }
    }

    public record AddToWatchLaterCommandPlaylistEditEndpoint
    {
        [JsonProperty("playlistId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlaylistId { get; set; }

        [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
        public List<TentacledAction> Actions { get; set; }
    }

    public record TentacledAction
    {
        [JsonProperty("addedVideoId", NullValueHandling = NullValueHandling.Ignore)]
        public string AddedVideoId { get; set; }

        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }
    }

    public record GetSharePanelCommand
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("webPlayerShareEntityServiceEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public WebPlayerShareEntityServiceEndpoint WebPlayerShareEntityServiceEndpoint { get; set; }
    }

    public record WebPlayerShareEntityServiceEndpoint
    {
        [JsonProperty("serializedShareEntity", NullValueHandling = NullValueHandling.Ignore)]
        public string SerializedShareEntity { get; set; }
    }

    public record RemoveFromWatchLaterCommand
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("playlistEditEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public RemoveFromWatchLaterCommandPlaylistEditEndpoint PlaylistEditEndpoint { get; set; }
    }

    public record RemoveFromWatchLaterCommandPlaylistEditEndpoint
    {
        [JsonProperty("playlistId", NullValueHandling = NullValueHandling.Ignore)]
        public string PlaylistId { get; set; }

        [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
        public List<StickyAction> Actions { get; set; }
    }

    public record StickyAction
    {
        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("removedVideoId", NullValueHandling = NullValueHandling.Ignore)]
        public string RemovedVideoId { get; set; }
    }

    public record SubscribeCommand
    {
        [JsonProperty("clickTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickTrackingParams { get; set; }

        [JsonProperty("commandMetadata", NullValueHandling = NullValueHandling.Ignore)]
        public ServiceEndpointCommandMetadata CommandMetadata { get; set; }

        [JsonProperty("subscribeEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public SubscribeEndpoint SubscribeEndpoint { get; set; }
    }

    public record ResponseContext
    {
        [JsonProperty("visitorData", NullValueHandling = NullValueHandling.Ignore)]
        public string VisitorData { get; set; }

        [JsonProperty("serviceTrackingParams", NullValueHandling = NullValueHandling.Ignore)]
        public List<ServiceTrackingParam> ServiceTrackingParams { get; set; }

        [JsonProperty("mainAppWebResponseContext", NullValueHandling = NullValueHandling.Ignore)]
        public MainAppWebResponseContext MainAppWebResponseContext { get; set; }

        [JsonProperty("webResponseContextExtensionData", NullValueHandling = NullValueHandling.Ignore)]
        public WebResponseContextExtensionData WebResponseContextExtensionData { get; set; }
    }

    public record MainAppWebResponseContext
    {
        [JsonProperty("loggedOut", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LoggedOut { get; set; }
    }

    public record ServiceTrackingParam
    {
        [JsonProperty("service", NullValueHandling = NullValueHandling.Ignore)]
        public string Service { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public List<Param> Params { get; set; }
    }

    public record Param
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public record WebResponseContextExtensionData
    {
        [JsonProperty("hasDecorated", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasDecorated { get; set; }
    }

    public record Storyboards
    {
        [JsonProperty("playerStoryboardSpecRenderer", NullValueHandling = NullValueHandling.Ignore)]
        public PlayerStoryboardSpecRenderer PlayerStoryboardSpecRenderer { get; set; }
    }

    public record PlayerStoryboardSpecRenderer
    {
        [JsonProperty("spec", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Spec { get; set; }
    }

    public record StreamingData
    {
        [JsonProperty("expiresInSeconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ExpiresInSeconds { get; set; }

        [JsonProperty("formats", NullValueHandling = NullValueHandling.Ignore)]
        public Format[] Formats { get; set; }

        [JsonProperty("adaptiveFormats", NullValueHandling = NullValueHandling.Ignore)]
        public AdaptiveFormat[] AdaptiveFormats { get; set; }

        [JsonProperty("probeUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ProbeUrl { get; set; }
    }

    public record AdaptiveFormat
    {
        [JsonProperty("itag", NullValueHandling = NullValueHandling.Ignore)]
        public long? Itag { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
        public string MimeType { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public long? Bitrate { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("initRange", NullValueHandling = NullValueHandling.Ignore)]
        public Range InitRange { get; set; }

        [JsonProperty("indexRange", NullValueHandling = NullValueHandling.Ignore)]
        public Range IndexRange { get; set; }

        [JsonProperty("lastModified", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModified { get; set; }

        [JsonProperty("contentLength", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ContentLength { get; set; }

        [JsonProperty("quality", NullValueHandling = NullValueHandling.Ignore)]
        public string Quality { get; set; }

        [JsonProperty("fps", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fps { get; set; }

        [JsonProperty("qualityLabel", NullValueHandling = NullValueHandling.Ignore)]
        public string QualityLabel { get; set; }

        [JsonProperty("projectionType", NullValueHandling = NullValueHandling.Ignore)]
        public ProjectionType? ProjectionType { get; set; }

        [JsonProperty("averageBitrate", NullValueHandling = NullValueHandling.Ignore)]
        public long? AverageBitrate { get; set; }

        //[JsonProperty("colorInfo", NullValueHandling = NullValueHandling.Ignore)]
        //public ColorInfo ColorInfo { get; set; }

        [JsonProperty("approxDurationMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ApproxDurationMs { get; set; }

        [JsonProperty("highReplication", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HighReplication { get; set; }

        [JsonProperty("audioQuality", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioQuality { get; set; }

        [JsonProperty("audioSampleRate", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? AudioSampleRate { get; set; }

        [JsonProperty("audioChannels", NullValueHandling = NullValueHandling.Ignore)]
        public long? AudioChannels { get; set; }

        [JsonProperty("loudnessDb", NullValueHandling = NullValueHandling.Ignore)]
        public double? LoudnessDb { get; set; }
    }

    //public record ColorInfo
    //{
    //    [JsonProperty("primaries", NullValueHandling = NullValueHandling.Ignore)]
    //    public Primaries? Primaries { get; set; }

    //    [JsonProperty("transferCharacteristics", NullValueHandling = NullValueHandling.Ignore)]
    //    public TransferCharacteristics? TransferCharacteristics { get; set; }

    //    [JsonProperty("matrixCoefficients", NullValueHandling = NullValueHandling.Ignore)]
    //    public MatrixCoefficients? MatrixCoefficients { get; set; }
    //}

    public record Range
    {
        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? Start { get; set; }

        [JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? End { get; set; }
    }

    public record Format
    {
        [JsonProperty("itag", NullValueHandling = NullValueHandling.Ignore)]
        public long? Itag { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        [JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
        public string MimeType { get; set; }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public long? Bitrate { get; set; }

        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public long? Width { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("lastModified", NullValueHandling = NullValueHandling.Ignore)]
        public string LastModified { get; set; }

        [JsonProperty("contentLength", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ContentLength { get; set; }

        [JsonProperty("quality", NullValueHandling = NullValueHandling.Ignore)]
        public string Quality { get; set; }

        [JsonProperty("fps", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fps { get; set; }

        [JsonProperty("qualityLabel", NullValueHandling = NullValueHandling.Ignore)]
        public string QualityLabel { get; set; }

        [JsonProperty("projectionType", NullValueHandling = NullValueHandling.Ignore)]
        public ProjectionType? ProjectionType { get; set; }

        [JsonProperty("averageBitrate", NullValueHandling = NullValueHandling.Ignore)]
        public long? AverageBitrate { get; set; }

        [JsonProperty("audioQuality", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioQuality { get; set; }

        [JsonProperty("approxDurationMs", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ApproxDurationMs { get; set; }

        [JsonProperty("audioSampleRate", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? AudioSampleRate { get; set; }

        [JsonProperty("audioChannels", NullValueHandling = NullValueHandling.Ignore)]
        public long? AudioChannels { get; set; }
    }

    public record VideoDetails
    {
        [JsonProperty("videoId", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoId { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("lengthSeconds", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? LengthSeconds { get; set; }

        [JsonProperty("keywords", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Keywords { get; set; }

        [JsonProperty("channelId", NullValueHandling = NullValueHandling.Ignore)]
        public string ChannelId { get; set; }

        [JsonProperty("isOwnerViewing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOwnerViewing { get; set; }

        [JsonProperty("shortDescription", NullValueHandling = NullValueHandling.Ignore)]
        public string ShortDescription { get; set; }

        [JsonProperty("isCrawlable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsCrawlable { get; set; }

        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public WatermarkClass Thumbnail { get; set; }

        [JsonProperty("averageRating", NullValueHandling = NullValueHandling.Ignore)]
        public double? AverageRating { get; set; }

        [JsonProperty("allowRatings", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AllowRatings { get; set; }

        [JsonProperty("viewCount", NullValueHandling = NullValueHandling.Ignore)]
        // [JsonConverter(typeof(ParseStringConverter))]
        public long? ViewCount { get; set; }

        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public string Author { get; set; }

        [JsonProperty("isPrivate", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPrivate { get; set; }

        [JsonProperty("isUnpluggedCorpus", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsUnpluggedCorpus { get; set; }

        [JsonProperty("isLiveContent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsLiveContent { get; set; }
    }

    public enum MatrixCoefficients { ColorMatrixCoefficientsBt709 };

    public enum Primaries { ColorPrimariesBt709 };

    public enum TransferCharacteristics { ColorTransferCharacteristicsBt709 };

    public enum ProjectionType { Rectangular };

    //internal static record Converter
    //{
    //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    //    {
    //        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
    //        DateParseHandling = DateParseHandling.None,
    //        Converters =
    //        {
    //            MatrixCoefficientsConverter.Singleton,
    //            PrimariesConverter.Singleton,
    //            TransferCharacteristicsConverter.Singleton,
    //            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
    //        },
    //    };
    //}

    //internal record ParseStringConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        long l;
    //        if (Int64.TryParse(value, out l))
    //        {
    //            return l;
    //        }
    //        throw new Exception("Cannot unmarshal type long");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (long)untypedValue;
    //        serializer.Serialize(writer, value.ToString());
    //        return;
    //    }

    //    public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    //}

    //internal record MatrixCoefficientsConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(MatrixCoefficients) || t == typeof(MatrixCoefficients?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "COLOR_MATRIX_COEFFICIENTS_BT709")
    //        {
    //            return MatrixCoefficients.ColorMatrixCoefficientsBt709;
    //        }
    //        throw new Exception("Cannot unmarshal type MatrixCoefficients");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (MatrixCoefficients)untypedValue;
    //        if (value == MatrixCoefficients.ColorMatrixCoefficientsBt709)
    //        {
    //            serializer.Serialize(writer, "COLOR_MATRIX_COEFFICIENTS_BT709");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type MatrixCoefficients");
    //    }

    //    public static readonly MatrixCoefficientsConverter Singleton = new MatrixCoefficientsConverter();
    //}

    //internal record PrimariesConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(Primaries) || t == typeof(Primaries?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "COLOR_PRIMARIES_BT709")
    //        {
    //            return Primaries.ColorPrimariesBt709;
    //        }
    //        throw new Exception("Cannot unmarshal type Primaries");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (Primaries)untypedValue;
    //        if (value == Primaries.ColorPrimariesBt709)
    //        {
    //            serializer.Serialize(writer, "COLOR_PRIMARIES_BT709");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type Primaries");
    //    }

    //    public static readonly PrimariesConverter Singleton = new PrimariesConverter();
    //}

    //internal record TransferCharacteristicsConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(TransferCharacteristics) || t == typeof(TransferCharacteristics?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "COLOR_TRANSFER_CHARACTERISTICS_BT709")
    //        {
    //            return TransferCharacteristics.ColorTransferCharacteristicsBt709;
    //        }
    //        throw new Exception("Cannot unmarshal type TransferCharacteristics");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (TransferCharacteristics)untypedValue;
    //        if (value == TransferCharacteristics.ColorTransferCharacteristicsBt709)
    //        {
    //            serializer.Serialize(writer, "COLOR_TRANSFER_CHARACTERISTICS_BT709");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type TransferCharacteristics");
    //    }

    //    public static readonly TransferCharacteristicsConverter Singleton = new TransferCharacteristicsConverter();
    //}

    //internal record ProjectionTypeConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type t) => t == typeof(ProjectionType) || t == typeof(ProjectionType?);

    //    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null) return null;
    //        var value = serializer.Deserialize<string>(reader);
    //        if (value == "RECTANGULAR")
    //        {
    //            return ProjectionType.Rectangular;
    //        }
    //        throw new Exception("Cannot unmarshal type ProjectionType");
    //    }

    //    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    //    {
    //        if (untypedValue == null)
    //        {
    //            serializer.Serialize(writer, null);
    //            return;
    //        }
    //        var value = (ProjectionType)untypedValue;
    //        if (value == ProjectionType.Rectangular)
    //        {
    //            serializer.Serialize(writer, "RECTANGULAR");
    //            return;
    //        }
    //        throw new Exception("Cannot marshal type ProjectionType");
    //    }
    //}

    #endregion

    public static class Patterns
    {
        public static string AdaptiveFormats1 { get; set; } = @"(?<=""adaptiveFormats"":)(.*?)(?=\])";
        public static string AdaptiveFormats2 { get; set; } = @"(?<=\{"")(.*?)(?=},\{)";
        public static string SignatureCipher { get; set; } = @"(?<=""signatureCipher"":"")(.*?)(?="")";
        public static string FileURL { get; set; } = @"(?<=""url"":"")(.*?)(?="")";
        public static string JsURL { get; set; } = @"(?<=""jsUrl"":"")(.*?)(?="",)";
        public static string JsFunctionPattern1 { get; set; } = @"(?<sig>[a-zA-Z0-9$]{2})\s*=\s*function\(\s*a\s*\)\s*{\s*a\s*=\s*a\.split\(\s*""""\s*\).*};";
        public static string JsFunctionPattern2 { get; set; } = @"=function\(([a-zA-Z]+?)\)\{(.+?)\};";
        public static string JsFunctionPattern3 { get; set; } = @"=\{(.+?)\};";
        public static string YtInitialData { get; set; } = @"(?<=ytInitialData = )(.*?)(?=;)";
        public static string VideoRendererBlock { get; set; } = @"(\{""videoRenderer"":)(.*?)(?=,\{""videoRenderer"":)";
        public static string VideoName { get; set; } = @"(?<=""text"":"")(.*?)(?=""\}\])";
        public static string VideoID { get; set; } = @"(?<=\{""videoId"":"")(.*?)(?="")";
        public static string VideoViewCount { get; set; } = @"(?<=""viewCountText"":\{""simpleText"":"")(.*?)(?="")";
        public static string PlaylistRenderer { get; set; } = @"(\{""playlistRenderer"":)(.*?)(?=,\{""playlistRenderer"":)";
        public static string PlaylistName { get; set; } = @"(?<=""simpleText"":"")(.*?)(?=""\},)";
        public static string PlaylistVideoCount { get; set; } = @"(?<=""videoCount"":"")(.*?)(?="")";
        public static string PlaylistID { get; set; } = @"(?<=""playlistId"":"")(.*?)(?="")";
        public static string YoutubeInitialResponse { get; set; } = "(?<=ytInitialPlayerResponse\\s*=)\\s*({.+?})\\s*(?=;)";
    }
    public class YouTubeAudioUrlUtil
    {
        public static int PartialIndexOf(string[] strings, string searchFor)
        {
            for (int i = 0; i < strings.Length; i++)
            {
                if (strings[i].IndexOf(searchFor) != -1)
                    return i;
            }
            return -1;
        }

        public static Dictionary<string, string> cifraToDict(string s)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            string[] s1 = s.Split('&');
            for (int i = 0; i < s1.Length; i++)
            {
                string[] s2 = s1[i].Split(new[] { '=' }, 2);
                if (s2.Length > 1)
                {
                    d.Add(s2[0], Uri.UnescapeDataString(s2[1]));
                }
            }
            return d;
        }

        public string Decipher(string id)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = null;
                    //downloads the whole page
                    string dpage = wc.DownloadString(string.Concat("https://www.youtube.com/watch?v=", id));

                    Regex dreg = new Regex(Patterns.YoutubeInitialResponse);
                    Match dm;

                    string _playerResponse = (dreg.Match(dpage).Value);
                    dreg = new Regex(Patterns.AdaptiveFormats1);

                    string AdaptiveFormats = dreg.Match((_playerResponse)).Value;
                    dreg = new Regex(Patterns.AdaptiveFormats2);
                    string[] mc = dreg.Matches(AdaptiveFormats).Cast<Match>().Select(m => m.Value).ToArray();
                    int AudioMaisProximo = PartialIndexOf(mc, "audio/");
                    if (AudioMaisProximo > -1)
                    {
                        dreg = new Regex(Patterns.SignatureCipher);
                        MatchCollection SignatureCipher = dreg.Matches(mc[AudioMaisProximo]);
                        if (SignatureCipher.Count > 0)
                        {
                            string r_cifra = Regex.Unescape(SignatureCipher[0].Value);
                            Dictionary<string, string> CipherDetails = cifraToDict(r_cifra);

                            //match the player .js file
                            dreg = new Regex(Patterns.JsURL);
                            dm = dreg.Match(dpage);
                            string BasePlayer = Regex.Unescape(dm.Groups[1].Value);
                            string djloc = string.Concat("https://youtube.com", BasePlayer);
                            string djs = wc.DownloadString(djloc);

                            //match the descrambling function in the downloaded javascript
                            dreg = new Regex(Patterns.JsFunctionPattern1);
                            //(?:\b|[^a-zA-Z0-9$])(?P<sig>[a-zA-Z0-9$]{2})\s*=\s*function\(\s*a\s*\)\s*{\s*a\s*=\s*a\.split\(\s*\"\"\s*\).*};
                            dm = dreg.Match(djs);
                            string dfunc = dm.Groups[1].Value;
                            dreg = new Regex(string.Concat(Regex.Escape(dfunc), Patterns.JsFunctionPattern2));
                            dm = dreg.Match(djs);

                            string dargn = dm.Groups[1].Value;
                            string dalg = dm.Groups[2].Value;

                            //pretty prints it for later usage
                            string djsfunc = string.Concat("var unscramble = function(", dargn, ") { ", dalg, " };");
                            string[] dalgps = dalg.Split(';');
                            HashSet<string> dalgrs = new HashSet<string>();
                            foreach (string dalgp in dalgps)
                                if (!dalgp.StartsWith(string.Concat(dargn, "=")) && !dalgp.StartsWith("return "))
                                    dalgrs.Add(dalgp.Split('.')[0]);

                            dreg = new Regex(string.Concat("var ", dalgrs.Where(c => !c.Contains(")")).FirstOrDefault(), Patterns.JsFunctionPattern3), RegexOptions.Singleline);
                            dm = dreg.Match(djs);

                            dalg = dm.Groups[0].Value;
                            djsfunc = string.Concat(dalg, "\n", djsfunc, "");

                            //instantiates the JS engine,calls the js function and returns the deciphered URL
                            Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();
                            engine.Evaluate(djsfunc);
                            string UnscambledCipher = (engine.CallGlobalFunction<string>("unscramble", CipherDetails["s"]));

                            if (!string.IsNullOrEmpty(UnscambledCipher))
                            {
                                return Uri.UnescapeDataString($"{CipherDetails["url"]}&{CipherDetails["sp"]}={UnscambledCipher}");
                            }
                            else
                            {
                                return string.Empty;
                            }

                        }
                        else
                        {
                            dreg = new Regex(Patterns.FileURL);
                            MatchCollection URL = dreg.Matches(mc[AudioMaisProximo]);
                            if (URL.Count > 0)
                            {
                                return Uri.UnescapeDataString(Regex.Unescape(URL[0].Value));
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return "";
            }
        }

        public async Task<Format> GetYtAudioUrl(RestClient client, string id)
        {

            IRestRequest request = new RestRequest(Method.POST);
            request.Resource = "https://youtubei.googleapis.com/youtubei/v1/player?key=AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";

            YouTubeVideoRequest utube = new YouTubeVideoRequest
            {
                context = new Context
                {
                    client = new Client
                    {
                        hl = "en",
                        clientName = "WEB",
                        clientVersion = "2.20210721.00.00",
                        mainAppWebInfo = new Mainappwebinfo
                        {
                            graftUrl = $"/watch?v={id}"
                        }
                    }
                },
                videoId = id
            };
            request.AddJsonBody(utube);

            IRestResponse response = await new RestClient().ExecuteAsync(request).ConfigureAwait(false);
            YouTubeResponse youTube = JsonConvert.DeserializeObject<YouTubeResponse>(response.Content);
            Console.WriteLine(JObject.FromObject(youTube.StreamingData).ToString());
            return youTube.StreamingData.Formats.FirstOrDefault();
        }
    }

    public class MusicCommands : BaseCommandModule
    {
        public ServicesContainer Services { private get; set; }

        [Command("search")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Search(CommandContext ctx, [RemainingText] string url)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var loadResult = await node.Rest.GetTracksAsync(url).ConfigureAwait(false);
            var track = loadResult.Tracks.First();

            //YouTubeAudioUrlUtil utube = new YouTubeAudioUrlUtil();
            //var response = utube.Decipher(track.Identifier);

            var ytdl = new YoutubeDL();
            // set the path of the youtube-dl and FFmpeg if they're not in PATH or current directory
            ytdl.YoutubeDLPath = "Binaries\\youtube-dl.exe";
            ytdl.FFmpegPath = "Binaries\\ffmpeg.exe";
            // optional: set a different download folder
            //ytdl.OutputFolder = "some\\directory\\for\\video\\downloads";
            // download a video
            var res = await ytdl.RunVideoDataFetch(track.Uri.ToString());
            // the path of the downloaded file
            FormatData path = res.Data.Formats.OrderByDescending(x => x.AudioBitrate).FirstOrDefault();

            using MemoryStream audioStream = new MemoryStream();
            using MediaFoundationReader mediaReader = new MediaFoundationReader(path.Url);
            if (mediaReader.CanRead)
            {
                // move to the beginning of the mediaReader stream
                mediaReader.Seek(0, SeekOrigin.Begin);

                // convert the audio track to Wave data and save to audioStream
                WaveFileWriter.WriteWavFileToStream(audioStream, mediaReader);

                audioStream.Seek(0, SeekOrigin.Begin);

                await ctx.RespondAsync(x => x.WithFile($"{res.Data.Title}.wav", audioStream).WithContent($"{res.Data.Title}:")).ConfigureAwait(false);
            }

            //foreach (var part in parts)
            //{
            //    await ctx.Channel.SendMessageAsync(new string(part)).ConfigureAwait(false);
            //}

        }

        [Command("song")]
        //[Aliases("n")]
        [Description("A new command")]
        public async Task Song(CommandContext ctx, [RemainingText] string songName)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            var Genius = Services.GeniusAPI;
            var Search = await Genius.SearchClient.Search(songName).ConfigureAwait(false);

            var hits = Search.Response.Hits;

            int ResCount = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var hit in hits)
            {
                var lyrics = await Genius.SongClient.GetSong(hit.Result.Id).ConfigureAwait(false);
                //sb.AppendLine($"{hit.Result.Title}");
                if (hit.Result.PrimaryArtist.Name != null)
                {
                    ResCount++;
                    if (hit.Result.LyricsState != null)
                    {
                        sb.AppendLine(
                            $"{ResCount}) {hit.Result.PrimaryArtist.Name} - {hit.Result.Title}");
                    }
                    else
                    {
                        sb.AppendLine($"{ResCount}) {hit.Result.PrimaryArtist.Name} - {hit.Result.Title}");
                    }
                }
            }

            Console.WriteLine($"{sb.ToString()}");

            await ctx.RespondAsync($"```{sb.ToString()}```").ConfigureAwait(false);
        }

        [Command("lyrics")]
        //[Aliases("n")]
        [Description("\nFind lyrics for any song you like (or hate)! If no song name is specified, the bot will see if you are listening to a song on Spotify and check what song you are listening to.")]
        public async Task Lyrics(CommandContext ctx, [RemainingText, Description("The name of the song. [If not using the Spotify activity status]")] string SongName = null)
        {
            await ctx.TriggerTypingAsync().ConfigureAwait(false);

            if ((ctx.User.Presence.Activities is not null
                && ctx.User.Presence.Activities.Select(x => x.ActivityType).Contains(ActivityType.ListeningTo))
                || !string.IsNullOrWhiteSpace(SongName))
            {
                DiscordActivity UserActivity =
                    ctx.User.Presence.Activities.Where(x => x.ActivityType == ActivityType.ListeningTo).FirstOrDefault();

                if (SongName == null
                    && UserActivity is not null
                    && UserActivity.ActivityType == ActivityType.ListeningTo
                    && UserActivity.Name.ToLower() == "spotify"
                    )
                    SongName = UserActivity.RichPresence.Details + " " + UserActivity.RichPresence.State;

                SearchResponse Search = await Services.GeniusAPI.SearchClient.Search(SongName).ConfigureAwait(false);
                Song hit = Search.Response.Hits[0].Result;

                List<string> Lyrics = await hit.GenerateLyricsParagraphs(Services.HttpClient).ConfigureAwait(false);
                List<string> Parts = Lyrics.Cast<string>().Where(x => x.Length > 1).ToList();

                if (Parts.Count < 25)
                    await ctx.RespondAsync(await GenerateLyricsEmbed(hit, Parts)).ConfigureAwait(false);
                else
                {
                    await ctx.RespondAsync
                        (
                            $"Woa! That seems like a very long song...\n" +
                            $"Unfotunatly, Discord prevents us (bots) from having more than 25 fields in embeds.\n" +
                            $"But don't let that destroy your mood!" +
                            $"If you still want to see the lyrics for {hit.Title} you could still check out {hit.Url}"
                        )
                        .ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.RespondAsync($"Song was not supplied! See `{ctx.Prefix}help {ctx.Command.Name}` for more help with this command.").ConfigureAwait(false);
            }

        }

        private async Task<DiscordEmbedBuilder> GenerateLyricsEmbed(Song hit, List<string> Parts)
        {
            DiscordEmbedBuilder embed = null;

            if (Parts.Count < 25)
            {
                Color ArtistIconEC = await ColorMath.GetAverageColorByImageUrlAsync(hit.SongArtImageUrl, Services.HttpClient).ConfigureAwait(false);
                embed = new()
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    { Name = $"Lyrics for: {hit.FullTitle}", IconUrl = hit.PrimaryArtist.ImageUrl, Url = hit.Url },

                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    { Url = hit.SongArtImageUrl },

                    Color = new DiscordColor(ArtistIconEC.R, ArtistIconEC.G, ArtistIconEC.B),

                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    { Text = $"Source from: {hit.Url}", IconUrl = "https://images.genius.com/ba9fba1d0cdbb5e3f8218cbf779c1a49.300x300x1.jpg" }
                };

                foreach (string Part in Parts)
                {
                    string FirstLine = Part.SplitLines()[0].TrimStart().TrimEnd();
                    try
                    {
                        if (FirstLine.StartsWith("[") && FirstLine.EndsWith("]"))
                        {
                            if (Part.Split(FirstLine)[1].Length > 1024)
                            {
                                embed.AddField(FirstLine, (Part.Split(FirstLine)[1]).Substring(0, 1024));
                                embed.AddField($"{FirstLine.Replace("]", "] - Second part")}", (Part.Split(FirstLine)[1])[1024..]);
                            }
                            else
                            {
                                embed.AddField(FirstLine, Part.Split(FirstLine)[1]);
                            }
                        }
                        else
                        {
                            if (Part.Contains("[") && Part.Contains("]"))
                                embed.AddField($"\u200b", Part.Replace("[", "**[").Replace("]", "]**"));
                            else
                                embed.AddField($"\u200b", Part);
                        }
                    }
                    catch (ArgumentException)
                    {

                    }
                }
            }

            return embed;
        }
    }
}