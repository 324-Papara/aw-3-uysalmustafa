using AutoMapper;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Query;

public class CustomerAddressQueryHandler :
    IRequestHandler<GetAllCustomerAddressesQuery, ApiResponse<List<CustomerAddressResponse>>>,
    IRequestHandler<GetCustomerAddressByIdQuery, ApiResponse<CustomerAddressResponse>>,
    IRequestHandler<GetCustomerAddressesByCustomerIdQuery, ApiResponse<List<CustomerAddressResponse>>>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public CustomerAddressQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public async Task<ApiResponse<List<CustomerAddressResponse>>> Handle(GetAllCustomerAddressesQuery request, CancellationToken cancellationToken)
    {
        List<CustomerAddress> entityList = await unitOfWork.CustomerAddressRepository.GetAll();
        var mappedList = mapper.Map<List<CustomerAddressResponse>>(entityList);
        return new ApiResponse<List<CustomerAddressResponse>>(mappedList);
    }

    public async Task<ApiResponse<CustomerAddressResponse>> Handle(GetCustomerAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.CustomerAddressRepository.GetById(request.AddressId);
        var mapped = mapper.Map<CustomerAddressResponse>(entity);
        return new ApiResponse<CustomerAddressResponse>(mapped);
    }

    public async Task<ApiResponse<List<CustomerAddressResponse>>> Handle(GetCustomerAddressesByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var addresses = await unitOfWork.CustomerAddressRepository.GetAll();
        var customerAddresses = addresses.Where(a => a.CustomerId == request.CustomerId).ToList();
        var mappedList = mapper.Map<List<CustomerAddressResponse>>(customerAddresses);
        return new ApiResponse<List<CustomerAddressResponse>>(mappedList);
    }
}