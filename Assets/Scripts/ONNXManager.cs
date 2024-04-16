using System.Collections;
using System;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;

public class ONNXManager : MonoBehaviour
{
    [SerializeField] private NNModel onnxModelAsset;
    [SerializeField] private Texture2D inputTexture;

    [Header("Result")]
    [SerializeField] private int predictedNumber;
    [SerializeField] private float[] numberPredictions;

    private Model runtimeModel;
    IWorker worker;

    // Start is called before the first frame update
    void Start()
    {
        runtimeModel = ModelLoader.Load(onnxModelAsset);
        worker = WorkerFactory.CreateWorker(runtimeModel, WorkerFactory.Device.Auto);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Process(inputTexture);
        }           
    }

    public void Process(Texture2D inputTexture)
    {
       Tensor inputTensor = new Tensor(inputTexture, 1);

       Tensor outputTensor = worker.Execute(inputTensor).PeekOutput();
       inputTensor.Dispose();
        GetResult(outputTensor);
    }

    private void GetResult(Tensor outputTensor)
    {
        float[] tensorValues = outputTensor.AsFloats();
        numberPredictions = SoftMaxCalculator(tensorValues);
        predictedNumber = Array.IndexOf(numberPredictions, numberPredictions.Max());
    }

    private float[] SoftMaxCalculator(float[] values)
    {
        var expValues = values.Select(x => Math.Exp(x));
        var sumValues = expValues.Sum();
        var softMaxValues = expValues.Select(v => (float)Math.Round(v/sumValues,4)).ToArray();
        return softMaxValues;
    }   

    private void OnDestroy()
    {
        worker.Dispose();
    }   
}
