using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database.Converters;

public class EntityToIdConverter() : ValueConverter<IEntity, long?>(entity => entity.Id,
    id => GlobalEntities.AllMarketTemplateEntities.SingleOrDefault(entity => entity.Id == id)!);