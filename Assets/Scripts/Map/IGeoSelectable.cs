namespace GeoVision.Map
{
    /// <summary>
    /// Any globe object that can be picked. PickController talks only to this contract.
    /// </summary>
    public interface IGeoSelectable
    {
        string Id { get; }
        string DisplayName { get; }
        string Type { get; }
        string Status { get; }
        string Description { get; }
        double Longitude { get; }
        double Latitude { get; }
        double Height { get; }

        void SetHighlighted(bool highlighted);
    }
}
