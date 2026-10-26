namespace CltPrueba.Api.Application.Addresses.Dto;

public record AddressDto(int Id, int UserId, string Street, string City, string Country, string? ZipCode);
