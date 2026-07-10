using WiiStudio.Application.DTOs;

namespace WiiStudio.Application.Interfaces;

public interface IWiimmsProgressParser
{
    bool TryParse(string line, out WiimmsOperationProgress progress);
}
