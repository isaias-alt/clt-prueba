namespace CltPrueba.Api.Application.Addresses.Dto;

public record AddressRequest(string Street, string City, string Country, string? ZipCode);
