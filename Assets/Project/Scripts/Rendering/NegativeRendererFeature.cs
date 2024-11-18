using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.RenderGraphModule;
    using UnityEngine.Rendering.RenderGraphModule.Util;
    using UnityEngine.Rendering.Universal;
    
    public class NegativeRenderPass : ScriptableRenderPass
    {
        private const string ShaderPath = "Hidden/Sample/Negative";
        private Material _material;
        private Material Material
        {
            get
            {
                if (_material == null)
                {
                    _material = CoreUtils.CreateEngineMaterial(ShaderPath);
                }
                return _material;
            }
        }
    
        public void Cleanup()
        {
            // ランタイム時生成したMaterialは手動で破棄する必要がある
            // これを忘れるとメモリリークが発生する
            CoreUtils.Destroy(_material);
        }
    
        private class PassData
        {
            public Material Material;
            public TextureHandle SourceTexture;
        }
    
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 解説 *1
            // frameDataからURP内蔵のリソースデータを取得
            var resourceData = frameData.Get<UniversalResourceData>();
    
            // 解説 *2
            // 入力とするテクスチャをresourceDataから取得
            var sourceTextureHandle = resourceData.activeColorTexture;  // activeColorTextureはカメラが描画したメインのカラーバッファ
    
            // 解説 *3
            // 出力用のテクスチャのDescriptorを作成
            var negativeDescriptor = renderGraph.GetTextureDesc(sourceTextureHandle);    // 入力テクスチャのDescriptorをコピー
            negativeDescriptor.name = "NegativeTexture";                                 // テクスチャの名前を設定
            negativeDescriptor.clearBuffer = false;                                      // クリア不要
            negativeDescriptor.msaaSamples = MSAASamples.None;                           // MSAA不要
            negativeDescriptor.depthBufferBits = 0;                                      // 深度バッファ不要
    
            // 解説 *4
            // Descriptorを用いて色反転テクスチャを作成
            var negativeTextureHandle = renderGraph.CreateTexture(negativeDescriptor);
    
            // 解説 *5
            // カメラカラーを反転し、出力用のテクスチャに描画するRasterRenderPassを作成し、RenderGraphに追加
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("NegativeRenderPass", out var passData))
            {
                // passDataに必要なデータを入れる
                passData.Material = Material;
                passData.SourceTexture = sourceTextureHandle;
    
                // 解説 *6
                // builderを通してRenderGraphPassに対して各種設定を行う
                // なお、描画ターゲットや他使用されるテクスチャは必ずこの段階で設定する必要がある
                builder.SetRenderAttachment(negativeTextureHandle, 0, AccessFlags.Write);    // 描画ターゲットに出力用のテクスチャを設定
                builder.UseTexture(sourceTextureHandle, AccessFlags.Read);                      // 入力テクスチャの使用を宣言する
    
                // 解説 *7
                // 実際の描画関数を設定する(static関数が推奨されてる)
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    // 解説 *8
                    // contextからCommandBufferを取得
                    var cmd = context.cmd;
                    var material = data.Material;
                    var source = data.SourceTexture;
                    // Blitterの便利関数を使ってBlit実行
                    Blitter.BlitTexture(cmd, source, Vector2.one, material, 0);
                });
            }
    
            // 解説 *9
            // 色反転テクスチャをカメラカラーにBlitする
            // 単純なBlitなら、RenderGraphは便利な関数を提供しているので、それを使う
            renderGraph.AddBlitPass(negativeTextureHandle, sourceTextureHandle, Vector2.one, Vector2.zero, passName: "BlitNegativeTextureToCameraColor");
        }
    }
    
    public class NegativeRendererFeature : ScriptableRendererFeature
    {
        private NegativeRenderPass _pass;
    
        public override void Create()
        {
            _pass = new NegativeRenderPass
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