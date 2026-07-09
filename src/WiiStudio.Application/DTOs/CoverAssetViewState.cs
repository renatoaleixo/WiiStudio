namespace WiiStudio.Application.DTOs;

public enum CoverAssetViewState
{
    Available = 1,
    Missing = 2,
    FromCache = 3,
    Downloading = 4,
    Failed = 5,
    InvalidImage = 6
}
