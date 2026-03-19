using MyApp.Models.Entities;
using MyApp.Services.DataContracts.Product;
using Riok.Mapperly.Abstractions;

namespace MyApp.Services.Mapper;

[Mapper]
public partial class ProductMapper
{
    [MapperIgnoreSource(nameof(Product.Description))]
    [MapperIgnoreSource(nameof(Product.CreatedAt))]
    [MapperIgnoreSource(nameof(Product.UpdatedAt))]
    public partial ProductResponse ToResponse(Product entity);

    public partial ProductResponseDetail ToDetailResponse(Product entity);

    [MapperIgnoreSource(nameof(Product.Description))]
    [MapperIgnoreSource(nameof(Product.CreatedAt))]
    [MapperIgnoreSource(nameof(Product.UpdatedAt))]
    public partial ProductResponseListItem ToListItemResponse(Product entity);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.CreatedAt))]
    [MapperIgnoreTarget(nameof(Product.UpdatedAt))]
    public partial Product ToEntity(ProductRequestCreate request);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.CreatedAt))]
    [MapperIgnoreTarget(nameof(Product.UpdatedAt))]
    public partial void UpdateEntity(ProductRequestUpdate request, [MappingTarget] Product entity);
}
