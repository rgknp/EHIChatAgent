In order to prep dataset for training the model following steps are performed:
1. Unzip the file Epic EHI Tables.zip
2. Run ConvertHTMtoHTML.bat from utility folder to convert all .htm files to .html files
3. Upload the dataset to a Azure blob storage account
4. Create Azure Search AI Service and create an index for the dataset
5. Add Semantic search configuration to enable semanic search
6. Add the index as a datasource to Azure OpenAI Service