using DbLocator.Domain;

namespace DbLocator.Features.Connections.GetConnections;

internal class GetConnections(IConnectionRepository connectionRepository)
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;

    public async Task<List<Connection>> Execute()
    {
        return await _connectionRepository.GetConnections();
    }
}
