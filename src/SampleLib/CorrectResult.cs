using DynamicWebApi.Client;
using SampleLib;
using System;
using System.Threading.Tasks;

public class SampleWebImpl : WebApiBase, ISample
{
    public Task<SampleData> GetSample(int value)
    {
        return WebApiBase.PostAsync(this, "Sample1/GetSample", new { value }).AsResult<SampleData>();
    }
    
    public Task<SampleData> GetSample2(SampleData data)
    {
        return WebApiBase.PostAsync(this, "Sample1/GetSample2", new { data }).AsResult<SampleData>();
    }
    
    public Task<SampleData> GetNull(SampleData data)
    {
        return WebApiBase.PostAsync(this, "Sample1/GetNull", new { data }).AsResult<SampleData>();
    }
    
    public Task<int> GetValue(int value)
    {
        return WebApiBase.PostAsync(this, "Sample1/GetValue", new { value }).AsResult<int>();
    }
    
    public Task<string> GetMulti(int a, int b, string suf)
    {
        return WebApiBase.PostAsync(this, "Sample1/GetMulti", new { a, b, suf }).AsResult<string>();
    }
    
    public Task<string> Test()
    {
        return WebApiBase.PostAsync(this, "Sample1/Test", new { }).AsResult<string>();
    }
    
    public Task<int> Increment()
    {
        return WebApiBase.PostAsync(this, "Sample1/Increment", new { }).AsResult<int>();
    }
    
    public Task<ContainerInfo[]> GetContainers()
    {
        return WebApiBase.PostAsync(this, "Sample1/GetContainers", new { }).AsResult<ContainerInfo[]>();
    }
    
    public Task<int> Exception()
    {
        return WebApiBase.PostAsync(this, "Sample1/Exception", new { }).AsResult<int>();
    }
    
    public Task<object> Nope()
    {
        return WebApiBase.PostAsync(this, "Sample1/Nope", new { }).AsResult<object>();
    }

    public Task<object> Delay(int delay)
    {
        return WebApiBase.PostAsync(this, "Sample1/Delay", new { delay }).AsResult<object>();
    }
}
