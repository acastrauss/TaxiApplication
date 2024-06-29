using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageWrapper.DTO
{
    public interface IDTOConverter<TA, TM> where TA : Entities.AzureBaseEntity
    {
        TM AzureToAppModel(TA azureModel);
        TA AppModelToAzure(TM appModel);
    }
}
