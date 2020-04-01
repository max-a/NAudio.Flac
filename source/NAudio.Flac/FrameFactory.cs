using System;
using System.IO;

namespace NAudio.Flac
{
	public class FrameFactory
	{
		private static FrameFactory _instance;

		public static FrameFactory Instance => _instance ?? (_instance = new FrameFactory());

		private FrameFactory()
		{
		}

		public Frame GetFrame(FrameHeader header, ID3Version version, Stream stream)
		{
			Frame frame = GetFrame(header.FrameID, version, header);
			frame.DecodeContent(ID3Utils.Read(stream, header.FrameSize));
			return frame;
		}

		public Frame TryGetFrame(FrameHeader header, ID3Version version, Stream stream, out bool result)
		{
			try
			{
				result = true;
				return GetFrame(header, version, stream);
			}
			catch (Exception)
			{
				result = false;
				return null;
			}
		}

		private Frame GetFrame(string id, ID3Version version, FrameHeader header)
		{
			switch (FrameIDFactory2.GetFrameEntry(id, version).ID)
			{
			case FrameID.InvolvedPeopleList:
			case FrameID.Album:
			case FrameID.Composer:
			case FrameID.ContentType:
			case FrameID.CopyrightMessage:
			case FrameID.EncodedBy:
			case FrameID.TextWriter:
			case FrameID.FileType:
			case FrameID.ContentGroupDescription:
			case FrameID.Title:
			case FrameID.Subtitle:
			case FrameID.InitialKey:
			case FrameID.Languages:
			case FrameID.Length:
			case FrameID.MediaType:
			case FrameID.OriginalAlbum:
			case FrameID.OriginalFileName:
			case FrameID.OriginalTextWriter:
			case FrameID.OriginalArtist:
			case FrameID.FileOwner:
			case FrameID.LeadPerformers:
			case FrameID.Band:
			case FrameID.Conductor:
			case FrameID.Interpreted:
			case FrameID.PartOfASet:
			case FrameID.Publisher:
			case FrameID.TrackNumber:
			case FrameID.RecordingDates:
			case FrameID.InternetRadioStationName:
			case FrameID.InternetRadioStationOwner:
			case FrameID.ISRC:
			case FrameID.EncodingSettings:
			case FrameID.MusicicanCreditsList:
			case FrameID.Mood:
			case FrameID.ProducedNotice:
			case FrameID.AlbumSortOrder:
			case FrameID.PerformerSortOrder:
			case FrameID.TitleSortOrder:
			case FrameID.SetSubtitle:
				return new MultiStringTextFrame(header);
			case FrameID.BeatsPerMinute:
			case FrameID.Date:
			case FrameID.PlaylistDelay:
			case FrameID.Time:
			case FrameID.OriginalReleaseYear:
			case FrameID.Size:
			case FrameID.Year:
				return new NumericTextFrame(header);
			case FrameID.OriginalReleaseTime:
			case FrameID.ReleaseTime:
			case FrameID.RecordingTime:
			case FrameID.EncodingTime:
			case FrameID.TaggingTime:
				return new TimestampTextFrame(header);
			case FrameID.CommercialInformationURL:
			case FrameID.CopyrightURL:
			case FrameID.OfficialAudioFileWebpage:
			case FrameID.OfficialArtistWebpage:
			case FrameID.OfficialAudioSourceWebpage:
			case FrameID.InternetRadioStationWebpage:
			case FrameID.PaymentURL:
			case FrameID.PublishersOfficialWebpage:
				return new TextFrame(header);
			case FrameID.PrivateFrame:
			case FrameID.UniqueFileIdentifier:
				return new DataFrame(header);
			case FrameID.EncryptedMetaData:
			case FrameID.Equalization:
			case FrameID.EqualizationOld:
			case FrameID.MusicCDIdentifier:
			case FrameID.MPEGLocationLookupTable:
			case FrameID.RecommendedBufferSize:
			case FrameID.RelativeVolumeAdjustment:
			case FrameID.RelativeVolumeAdjustmentOld:
			case FrameID.SynchronizedTempoCodes:
			case FrameID.AudioSeekPointIndex:
				return new BinaryFrame(header);
			case FrameID.UserTextInformation:
			case FrameID.UserURLLinkFrame:
				return new UserDefiniedTextFrame(header);
			case FrameID.AttachedPicutre:
				return new PictureFrame(header, version);
			case FrameID.Comments:
			case FrameID.UnsynchronizedLyris:
				return new CommentAndLyricsFrame(header);
			case FrameID.TermsOfUse:
				return new TermsOfUseFrame(header);
			case FrameID.OwnershipFrame:
				return new OwnershipFrame(header);
			case FrameID.CommercialFrame:
				return new CommercialFrame(header);
			case FrameID.Popularimeter:
				return new Popularimeter(header);
			default:
				return null;
			}
		}
	}
}
