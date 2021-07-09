namespace Core.Contracts {
    public interface IDataModelConverter<TOutput> {
        TOutput Convert(byte[] data);
    }
}
