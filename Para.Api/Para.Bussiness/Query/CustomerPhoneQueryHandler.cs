using AutoMapper;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;
using System.Linq;

namespace Para.Bussiness.Query;

public class CustomerPhoneQueryHandler :
    IRequestHandler<GetAllCustomerPhonesQuery, ApiResponse<List<CustomerPhoneResponse>>>,
    IRequestHandler<GetCustomerPhoneByIdQuery, ApiResponse<CustomerPhoneResponse>>,
    IRequestHandler<GetCustomerPhonesByCustomerIdQuery, ApiResponse<List<CustomerPhoneResponse>>>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public CustomerPhoneQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public async Task<ApiResponse<List<CustomerPhoneResponse>>> Handle(GetAllCustomerPhonesQuery request, CancellationToken cancellationToken)
    {
        List<CustomerPhone> entityList = await unitOfWork.CustomerPhoneRepository.GetAll();
        var mappedList = mapper.Map<List<CustomerPhoneResponse>>(entityList);
        return new ApiResponse<List<CustomerPhoneResponse>>(mappedList);
    }

    public async Task<ApiResponse<CustomerPhoneResponse>> Handle(GetCustomerPhoneByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.CustomerPhoneRepository.GetById(request.PhoneId);
        var mapped = mapper.Map<CustomerPhoneResponse>(entity);
        return new ApiResponse<CustomerPhoneResponse>(mapped);
    }

    public async Task<ApiResponse<List<CustomerPhoneResponse>>> Handle(GetCustomerPhonesByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var phones = await unitOfWork.CustomerPhoneRepository.GetAll();
        var customerPhones = phones.Where(p => p.CustomerId == request.CustomerId).ToList();
        var mappedList = mapper.Map<List<CustomerPhoneResponse>>(customerPhones);
        return new ApiResponse<List<CustomerPhoneResponse>>(mappedList);
    }
}
