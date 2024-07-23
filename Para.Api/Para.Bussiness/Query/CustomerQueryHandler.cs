using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Query;

public class CustomerQueryHandler : 
    IRequestHandler<GetAllCustomerQuery,ApiResponse<List<CustomerResponse>>>,
    IRequestHandler<GetCustomerByIdQuery,ApiResponse<CustomerResponse>>,
    IRequestHandler<GetCustomerByParametersQuery,ApiResponse<List<CustomerResponse>>>
    
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public CustomerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }
    
    public async Task<ApiResponse<List<CustomerResponse>>> Handle(GetAllCustomerQuery request, CancellationToken cancellationToken)
    {
        List<Customer> entityList = await unitOfWork.CustomerRepository.GetAll();
        var mappedList = mapper.Map<List<CustomerResponse>>(entityList);
        return new ApiResponse<List<CustomerResponse>>(mappedList);
    }

    public async Task<ApiResponse<CustomerResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await unitOfWork.CustomerRepository.GetById(request.CustomerId);
        var mapped = mapper.Map<CustomerResponse>(entity);
        return new ApiResponse<CustomerResponse>(mapped);
    }

    public async Task<ApiResponse<List<CustomerResponse>>> Handle(GetCustomerByParametersQuery request, CancellationToken cancellationToken)
    {

        IQueryable<Customer> query = unitOfWork.CustomerRepository.AsQueryable();

        if (request.CustomerId > 0)
        {
            query = query.Where(c => c.Id == request.CustomerId);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            string lowerCaseName = request.Name.ToLower();
            query = query.Where(c => c.FirstName.ToLower().Contains(lowerCaseName) || c.LastName.ToLower().Contains(lowerCaseName));
        }


        List<Customer> customers = await query.ToListAsync(cancellationToken);
        List<CustomerResponse> customerResponses = mapper.Map<List<CustomerResponse>>(customers);
        return new ApiResponse<List<CustomerResponse>>(customerResponses);
    }
}