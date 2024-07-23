using AutoMapper;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Query;

public class CustomerDetailQueryHandler :
    IRequestHandler<GetAllCustomerDetailsQuery, ApiResponse<List<CustomerDetailResponse>>>,
    IRequestHandler<GetCustomerDetailByIdQuery, ApiResponse<CustomerDetailResponse>>,
    IRequestHandler<GetCustomerDetailsByCustomerIdQuery, ApiResponse<List<CustomerDetailResponse>>>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public CustomerDetailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public async Task<ApiResponse<List<CustomerDetailResponse>>> Handle(GetAllCustomerDetailsQuery request, CancellationToken cancellationToken)
    {
        List<CustomerDetail> entityList = await unitOfWork.CustomerDetailRepository.GetAll();
        var mappedList = mapper.Map<List<CustomerDetailResponse>>(entityList);
        return new ApiResponse<List<CustomerDetailResponse>>(mappedList);
    }

    public async Task<ApiResponse<CustomerDetailResponse>> Handle(GetCustomerDetailByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.CustomerDetailRepository.GetById(request.DetailId);
        var mapped = mapper.Map<CustomerDetailResponse>(entity);
        return new ApiResponse<CustomerDetailResponse>(mapped);
    }

    public async Task<ApiResponse<List<CustomerDetailResponse>>> Handle(GetCustomerDetailsByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var details = await unitOfWork.CustomerDetailRepository.GetAll();
        var customerDetails = details.Where(d => d.CustomerId == request.CustomerId).ToList();
        var mappedList = mapper.Map<List<CustomerDetailResponse>>(customerDetails);
        return new ApiResponse<List<CustomerDetailResponse>>(mappedList);
    }
}
