using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class NegativeRenderPass : ScriptableRenderPass
{
    private Material _material;

    public NegativeRenderPass(Material material)
    {
        _material = material;
    }

    public void Cleanup()
    {
        // ランタイム時生成したMaterialは手動で破棄する必要がある
        // これを忘れるとメモリリークが発生する
        // CoreUtils.Destroy(_material);
    }

    private class PassData
    {
        public Material Material;
        public TextureHandle SourceTexture;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // コンテナから必要なリソースを取得する
        
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        // 入力とするテクスチャをresourceDataから取得
        // activeColorTextureはカメラが描画したメインのカラーバッファ
        
        TextureHandle sourceTextureHandle = resourceData.activeColorTexture;

        // 出力用のテクスチャのDescriptorを作成
        
        TextureDesc negativeDescriptor = renderGraph.GetTextureDesc(sourceTextureHandle);
        negativeDescriptor.name = "NegativeTexture"; // テクスチャの名前を設定
        negativeDescriptor.clearBuffer = false; // クリア不要
        negativeDescriptor.msaaSamples = MSAASamples.None; // MSAA不要
        negativeDescriptor.depthBufferBits = 0; // 深度バッファ不要

        // Descriptorを用いて色反転テクスチャを作成
        
        TextureHandle negativeTextureHandle = renderGraph.CreateTexture(negativeDescriptor);

        // カメラカラーを反転し、出力用のテクスチャに描画するRasterRenderPassを作成し、RenderGraphに追加
        
        using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>("NegativeRenderPass", out PassData passData))
        {
            passData.Material = _material;
            passData.SourceTexture = sourceTextureHandle;

            // builderを通してRenderGraphPassに対して各種設定を行う
            // なお、描画ターゲットや他使用されるテクスチャは必ずこの段階で設定する必要がある
            builder.SetRenderAttachment(negativeTextureHandle, 0, AccessFlags.Write); // 描画ターゲットに出力用のテクスチャを設定
            builder.UseTexture(sourceTextureHandle, AccessFlags.Read); // 入力テクスチャの使用を宣言する

            // 実際の描画関数を設定する(static関数が推奨されてる)
            
            builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
            {
                RasterCommandBuffer cmd = context.cmd;
                Material material = data.Material;
                TextureHandle source = data.SourceTexture;
                
                Blitter.BlitTexture(cmd, source, Vector2.one, material, 0);
            });
        }

        // 色反転テクスチャをカメラカラーにBlitする
        // 単純なBlitなら、RenderGraphは便利な関数を提供しているので、それを使う
        
        renderGraph.AddBlitPass(negativeTextureHandle, sourceTextureHandle, Vector2.one, Vector2.zero, passName: "BlitNegativeTextureToCameraColor");
    }
}

public class NegativeRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material _material;
    
    private NegativeRenderPass _pass;

    public override void Create()
    {
        _pass = new NegativeRenderPass(_material)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // ここでPassの破棄処理を呼び出す
            _pass.Cleanup();
        }
    }
}