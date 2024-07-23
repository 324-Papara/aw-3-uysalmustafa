using AutoMapper;
using FluentValidation;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Command
{
    public class CustomerDetailCommandHandler :
        IRequestHandler<CreateCustomerDetailCommand, ApiResponse<CustomerDetailResponse>>,
        IRequestHandler<UpdateCustomerDetailCommand, ApiResponse>,
        IRequestHandler<DeleteCustomerDetailCommand, ApiResponse>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IValidator<CustomerDetail> validator;

        public CustomerDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CustomerDetail> validator)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.validator = validator;
        }

        public async Task<ApiResponse<CustomerDetailResponse>> Handle(CreateCustomerDetailCommand request, CancellationToken cancellationToken)
        {
            var customerDetail = mapper.Map<CustomerDetailRequest, CustomerDetail>(request.Request);
            var validationResult = await validator.ValidateAsync(customerDetail, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse<CustomerDetailResponse>(string.Join(", ", errorMessages));
            }

            await unitOfWork.CustomerDetailRepository.Insert(customerDetail);
            await unitOfWork.Complete();

            var response = mapper.Map<CustomerDetailResponse>(customerDetail);
            return new ApiResponse<CustomerDetailResponse>(response);
        }

        public async Task<ApiResponse> Handle(UpdateCustomerDetailCommand request, CancellationToken cancellationToken)
        {
            var customerDetail = mapper.Map<CustomerDetailRequest, CustomerDetail>(request.Request);
            customerDetail.Id = request.DetailId;

            var validationResult = await validator.ValidateAsync(customerDetail, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return new ApiResponse(string.Join(", ", errorMessages));
            }

            unitOfWork.CustomerDetailRepository.Update(customerDetail);
            await unitOfWork.Complete();
            return new ApiResponse();
        }

        public async Task<ApiResponse> Handle(DeleteCustomerDetailCommand request, CancellationToken cancellationToken)
        {
            await unitOfWork.CustomerDetailRepository.Delete(request.DetailId);
            await unitOfWork.Complete();
            return new ApiResponse();
        }
    }
}
