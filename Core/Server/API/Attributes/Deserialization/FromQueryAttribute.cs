using System.Collections.Specialized;
using System.ComponentModel;
using EmbedIO;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using Swan;

namespace Vint.Core.Server.API.Attributes.Deserialization;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromQueryAttribute(
    string? fieldName = null,
    object? @default = null,
    bool badRequestIfMissing = false
) : Attribute, IRequestDataAttribute<WebApiController, string>, IRequestDataAttribute<WebApiController, string[]>,
    IRequestDataAttribute<WebApiController> {

    string[]? GetValues(WebApiController controller, string key) {
        NameValueCollection query = controller.HttpContext.GetRequestQueryData();

        if (!query.ContainsKey(key) && badRequestIfMissing)
            throw HttpException.BadRequest($"Missing query field {key}");

        return query.GetValues(key);
    }

    T? GetDefaultValue<T>() => (T?)@default ?? default;

    object? GetDefaultValue(Type type) => @default ?? type.GetDefault();

    Task<string?> IRequestDataAttribute<WebApiController, string>.GetRequestDataAsync(WebApiController controller, string parameterName) =>
        Task.FromResult(GetValues(controller, fieldName ?? parameterName)?.LastOrDefault() ?? GetDefaultValue<string>());

    Task<string[]?> IRequestDataAttribute<WebApiController, string[]>.GetRequestDataAsync(WebApiController controller, string parameterName) =>
        Task.FromResult(GetValues(controller, fieldName ?? parameterName) ?? [])!;

    Task<object?> IRequestDataAttribute<WebApiController>.GetRequestDataAsync(WebApiController controller, Type type, string parameterName) {
        string key = fieldName ?? parameterName;
        string[] values = GetValues(controller, key) ?? [];
        TypeConverter converter;

        if (type.IsArray) {
            Type elementType = type.GetElementType()!;
            converter = TypeDescriptor.GetConverter(elementType);

            if (!converter.CanConvertFrom(typeof(string)))
                throw HttpException.BadRequest($"Cannot convert field {key} to an array of {elementType.FullName}");

            int length = values.Length;
            Array array = Array.CreateInstance(elementType, length);

            for (int i = 0; i < length; i++) {
                string strValue = values[i];
                object? val = converter.ConvertFromInvariantString(strValue);
                //object? val = Convert.ChangeType(values[i], elementType);
                array.SetValue(val, i);
            }

            return Task.FromResult<object?>(array);
        }

        string? value = values.LastOrDefault();

        if (value == null)
            return Task.FromResult(GetDefaultValue(type));

        converter = TypeDescriptor.GetConverter(type);

        if (!converter.CanConvertFrom(typeof(string)))
            throw HttpException.BadRequest($"Cannot convert field {key} to {type.FullName}");

        return Task.FromResult(converter.ConvertFromInvariantString(value));
    }
}
