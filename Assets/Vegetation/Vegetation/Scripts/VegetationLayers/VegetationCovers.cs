namespace Vegetation
{
    /// <summary>
    /// Tipos de vegetação descritos por coberturas vegetais.
    /// </summary>
    internal enum VegetationCover
    {
        NO_VEGETATION = -1,
        BIG_TREE = 0,
        MEDIUM_TREE = 1,
        BIG_BUSH = 2,
        MEDIUM_BUSH = 3,
        GROUND_VEGETATION = 4,
        DENSE_GROUND_VEGETATION = 5
    };

    /// <summary>
    /// Categorias das plantas que podem ser distribuidas na GPU para
    /// serem recuperadas e mantidas na RAM.
    /// </summary>
    /// <remarks>   
    /// Por motivos de custo de transferencias e elevado uso de memoria,
    /// algumas coberturas vegetais sao exclusivas à renderização.
    /// Dentro deste grupo restrito estão gramas e pequenos arbustos.
    /// </remarks>
    public enum CollectableVegetationCover
    {
        BIG_TREE = VegetationCover.BIG_TREE,
        MEDIUM_TREE = VegetationCover.MEDIUM_TREE,
        BIG_BUSH = VegetationCover.BIG_BUSH,
    };
}