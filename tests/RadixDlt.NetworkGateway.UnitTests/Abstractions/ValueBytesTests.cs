/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using FluentAssertions;
using RadixDlt.NetworkGateway.Abstractions;
using RadixDlt.NetworkGateway.GatewayApiSdk.Model;
using RadixDlt.NetworkGateway.PostgresIntegration;
using RadixDlt.NetworkGateway.PostgresIntegration.Utils;
using System;
using System.Collections.Generic;
using Xunit;

namespace RadixDlt.NetworkGateway.UnitTests.Abstractions;

public class ValueBytesTests
{
    [Fact]
    public void X()
    {
        var input = """
                    131,validator_rdx1sdmd9rptjr3g3rdu883h2rt8vqz4aqr2la65m8vkvevcwut0gmj3ua,0x5C220801805D4CFA59CD351D4823C37CDFE54809DD583E82838CC2C36C517D3F940D9E
                    132,validator_rdx1s0qtrarfm5eu9ewvtzud96q9mjm2u63qr99newtm6h28q9slmz9jdp,0x5C220801805DB146B395B46BD2550129281C6CA016D87D2E018F33CFF39A65DD1ABE3D
                    133,validator_rdx1svt63dxpmvgwcevvz8n0gv87x65fs5qkegygymuwnv0v8vuv33zcx3,0x5C220801805D08C8CC1952B79E9407C4D29F0875DE560539BBD21635DD41274E56CA25
                    134,validator_rdx1svr8rj5vsgpdlv5v39yq4zc9ummsgjqlsrrnmd09hwh0uwuhgg2ta7,0x5C220801805D3FAEF79A27CCEE085E08BA9976D35BADCE62A97F1BBCB7FEF135B93041
                    135,validator_rdx1sv63txjclt935gw8f5sfsz4rhh6v8ttelwwtlpgk85qjm4n57t0wzl,0x5C220801805DCB938125674798C1A96F173F372AF4877049D88E516C7167983B510BF1
                    136,validator_rdx1s0lz5v68gtqwswu7lrx9yrjte4ts0l2saphmplsz68nsv2aux0xvfq,0x5C220801805D0C549B4C912D437BCC58162C5CCDFDA812DD013B0223B2307B44DE3DBB
                    137,validator_rdx1sv7zpyuj27ycmcsnw0de94j2rp93csq6gvcm4dl4yh4p4fxv9j5pqx,0x5C220801805DD64332D69974D2779C74289F1D3A26597516946E71C5543172CC8C706C
                    138,validator_rdx1sv2nu2y6wmhcg4d99mjek5g8qmpc2ua73yfaz6tytgrasftamn9c2u,0x5C220801805D1B84AC33A0B62FAEF7887F1F06D05009EC7E2E34FFAA6775D459D42921
                    139,validator_rdx1swq3mn0u9qf68j6g3nugw3udakecggfgpu9e4yphe2rcqc32aavw9t,0x5C220801805DADDBF00A71E90E2324A35674CEFAA06236109DECA1E9EB01E26BDDA85A
                    140,validator_rdx1s0wwn928vcvd9kl72pgf2twcd4eqy5a2cl7804ed7p7pxthjjkc24n,0x5C220801805D7464CC63E8D8C4EEE9E1CCD53AA5CF3888792FDF79CF45DC6ABDDB2C3C
                    141,validator_rdx1sdxknlqv5cj55uj0e9dqc9aszy552hvsgqs5jaln4nzx9fvr92r87d,0x5C220801805DF409F7F44B45531D2DC85BC1987AD82D2C516AE06B821E475430F59A98
                    142,validator_rdx1s0drhvkx30k62zu0usnzxzwuh0qcsqwlc2n2kfyexlyghqpctuy2fx,0x5C220801805D7DA481329790579E711C0EB1186DE15B5B48127E4FDEF1B3E88284E166
                    143,validator_rdx1swnhuz9mqm6s7kkdjzxcgnavknltczrw9lr2m9pkfddh0hzg3dwt2f,0x5C220801805DC690F06C10E5975560200091A4A101DB8CF4C58DDF15606420357772C8
                    144,validator_rdx1sw5zkx2h6hp6k0js6dqaaxpz4580awncmm0rlzv7ufcf97cukjegy8,0x5C220801805D4B104EDC4B504A782F8D72A1CC9A44FDF49FB515CECD1D21CE21E67FDF
                    145,validator_rdx1s0n6dyl8cmzalflynrnevz8c26jffemp7xrd0fnc7a33ttdyx4q33q,0x5C220801805D8A05DFFAF067321E5BC08A3505581E2C3A1BB92E8776D229AD396270EB
                    146,validator_rdx1sd5rldutwtcmnlczj38n3hrhdqevu77wkqh64kn985nmvg2dxzmz7e,0x5C220801805D08DD97FA26768839F4501B533AB24075B8FD2118F6E84231A21877860A
                    147,validator_rdx1sv2lj628xcmsa20dy866w0vnpavduateww0h68avju67q2chkrmgps,0x5C220801805D819F649E4A38AFA32400096DA38941FF861BB04983D3CBD6EEB5F50ED3
                    148,validator_rdx1s0jjvkmgysp3hs8w2c90xmc87q725q9em760czp2nfp55jgje5rlp3,0x5C220801805DC622D98C1C2474D6C3640828FF34B5B333E58244A7A55D4E8D8E4F112E
                    149,validator_rdx1sv33rsnhwedawtsal4azz9m0j2eylcnutwtmz965cszks0es4g3q3l,0x5C220801805D6883FFCC43144D8802AFE1D5FC40B06D2DEF20F2B93821913EA0EA3EA0
                    150,validator_rdx1svptfyzdnzk3q0qccrg8vfyfnulm2nnuchtrr273cwhmxx84wfw8ah,0x5C220801805DA7CE5E0F723002C895B1E22A3FF821814CB19ECCFE05F9883B44DB0162
                    151,validator_rdx1sdauznj9luja8v7vd4jndmlp449tcr9qxkx6fkm87lxkn0lq6m9mn0,0x5C220801805D9B13C686CC986474F594C3593A7225B387AC0E76CA9A4DE83D1C881BBD
                    152,validator_rdx1svwrkf8anvjgwthgqzt6un35jflxlf9w0umepnz29sse90zewe75pf,0x5C220801805DE4728007654FC3B2EA5A95C18D1867E076678A354D7B83D08317CCD2B6
                    153,validator_rdx1svlkyg7d82v9dsnn7mpyu9yqtnjunc39y0hz7mn5mkj3r3luxc744y,0x5C220801805D758672843B9A87F678856679E823F1C999E6E670772C3568CF504C919A
                    154,validator_rdx1swwc4wy2pfghx9fvgprnk95nqlewt4c7gs39gef2j47td95gxhluec,0x5C220801805D35D8F34B4937B9EADF45E3A4A457B418AC52A3F59F31C5C58C8C3A84CB
                    155,validator_rdx1swzn5hvtut6yq0zxqqsa0wk4rnkfd8wewvnphzrckau22pun2lv86t,0x5C220801805DF5C9BF0986D7650A5D4126F279E3EDD8228AE4168FFB1D189AE25257BD
                    156,validator_rdx1swgaevyu0hj4qsrt4gqvk0ux7w2kmgh940j9swp8lh6a2exl3p558q,0x5C220801805D4ABEC1EBD795FCD1AA4837C94F3EBEF1EB41C35E3903534BE940F65871
                    157,validator_rdx1sdrs82eshqah25dttzend5cn48n9ztld6a6rjndc3xxmnarxtwzjt8,0x5C220801805D178BF6849508DB85BC8174FE8A2AD0BAE454FB874B36F2C5DD1ABD3A68
                    158,validator_rdx1svn4a93yzac2wlg6hv5qhptqsuqjkd5np4avm95ygfhqftk4rf93pt,0x5C220801805D015523F57DD17FFA45C04C9785028BB573F77E782EB01D9024490A08B0
                    159,validator_rdx1s0cdlvk5wpgklxnkap05jvgdskpa54yyfu232tfu5s65n7ya6mxhlg,0x5C220801805D8284E30FD47EC93653D40EC85AC2236F172591F31F9141F218F64E0FB9
                    160,validator_rdx1swujat7hw4s6cmx7vq6yk4fzxc6mnfmw4tpqyunlthacat55s7zd8w,0x5C220801805D8C440BFCAC673B009FAB56A8905F7ECCABC7571F0F0D68AFD4A3E08C1E
                    161,validator_rdx1s0llvys3mnwmgssx0flenq93lddxyfyaudfrcr7ass6flunzjgn2jl,0x5C220801805DD3716B6E1B34D0A0B2232929E499898ABDCFEF37CA12F055F23E966CB7
                    162,validator_rdx1swv0c3m7hdg7wm7mwfm4p8qhlldsq3e82qul5tmqzry20xf975dngc,0x5C220801805D3082B5E0621E34CCFF423B3D14014058F8AFF5E9BC450EEC116CA048D3
                    163,validator_rdx1sv0lcmlfvwkf8zp5x3q6g3d2rk6fd6kdxvluqe3v04rzdqew6vhvgs,0x5C220801805D012F1780F6E5B488FE40C12CDE1441D388F6AD3E7401614A2D451857C6
                    164,validator_rdx1sv5zyqk0tsma5gp4h690cug9e6ashvv3genfzf0ukevgj84t6vyn3x,0x5C220801805DB712CFCC43E22FC1F4C1F4B4589DD68E98E94F76B57C3DB4D85FA84512
                    165,validator_rdx1svug50cdlalm6plazajrmntf209j5azf57xeukuhx2hw7e7ut5mmz8,0x5C220801805DE274B6974124C56F669E1EAA0C4EA03FC2F9858D08ADF587449DF7CAFD
                    166,validator_rdx1s040h38utqrz8dxasq9advl9vk4mc03twutg9znhegslwchvsjye5a,0x5C220801805D058052E08EB29182387901054C8336CCDCBB253DACF6C9D822C5A03597
                    167,validator_rdx1sv0pdyu789fa5tj2m2r4an4dvz4mvfpax7ktg8r93tlcqe73qt6kw9,0x5C220801805DC947C3A0D3F9A462F0A13776B110FBEBC1755F3A9DE7710A3F5364B170
                    168,validator_rdx1sdwk0ty3zjsq5925q9qh0qmnex64vkf3khlz6vvhm8sn4nq596ecdf,0x5C220801805D80F3D8743EEF60B0A82D99096FC71AB024DC0E55F9EF7A2961D29999EA
                    169,validator_rdx1swckjtt6rd4fklja9nturvjevyryf397nnzta99lx5wkzkpsmn627c,0x5C220801805D4326EFB224ADCDA45612AF0D6C60E779922E50C300E5F63682776099B8
                    170,validator_rdx1sdk493n96m2v8t0st6hgrdmdua8y6kp84lcwv35qr7umvj7ar4x3e8,0x5C220801805DC7A3370CC6E5C01F8A1486ECEBEFC227324D47CB2544AD80DD7203575A
                    171,validator_rdx1sv8npgz089qm9ckljmfyveuy523m376rns54mfcrtz5az2tl3h5w6z,0x5C220801805DD6C98F76B6375E292E52DEC63744F8489201727A44B613CD055DD80C51
                    172,validator_rdx1s0ecyev32587zjyrc50lu900q64ykxcr7er8delatak6vkk9t4g4q0,0x5C220801805DC045A91A54A2B357B282E93CD41021C11D256D0FC85D9FFE6F5BD24C90
                    173,validator_rdx1sd0kp0vm3wedcdgv96yn4duu3gd2uc8vx4ddqem97ranm4ju907sqr,0x5C220801805DF8BE07B06F863B3FB5C4718DF82BEFC5F2CD45B9C20DC72D57A5F9363F
                    174,validator_rdx1svkevtze6t74sq9x09d0faddmu2xz2dp2qzurmafxxplcjgd2up6xu,0x5C220801805DA8F9044C5B08266E2A2C1D2C2046F08D73EB2D5D9A1336834852633471
                    175,validator_rdx1svkch0tjam57zrs72hu7gc2z93vaq5raqsg0z3vwqz6v46mc7rxtuz,0x5C220801805D080445D1B5702277DE18FB9EDFAE6CE84591472B1F093F18102B6BCBD4
                    176,validator_rdx1sv07myvl9hs935aadunwwmp0fy0jh8p2h555j6836et5etswnjjwd6,0x5C220801805DE7593B3429BD3EDB4A614BB607943DAE35EBEE819E5C21B719DE4CCFAD
                    177,validator_rdx1s0eeyn9uxs06te9rsddawhc629rg4tv2e4khukez7xxk29vlc0upwn,0x5C220801805D0BCE999D11518A942357CB3495814FA7E52375C09BF553875D75D1EBFA
                    178,validator_rdx1sv7wkvr4dqcwkwz42pyry6cszw7adcv5vzp8w57tx9l7cgark33juq,0x5C220801805D6ACBE8F5FC3C329103A3867ED62EE301238595B20CFA7DC46723CD8797
                    179,validator_rdx1sd9ewm5vgjrtw9axe6vpjg0emelx9pxfnnhq62m8jv8enknq99slyu,0x5C220801805D168C3C2FDE229D2445628A3205F955DD78D34EC774EE4252221CCC064E
                    180,validator_rdx1sdpcajykt7mcvqskec4n4mw7v3605l73nt779azumxh7v0nhpqsj95,0x5C220801805D1422E86D1ED5908C6D42917D65F95FD047CD9A5EC08AA27228BED0DEE6
                    181,validator_rdx1swkmn6yvrqjzpaytvug5fp0gzfy9zdzq7j7nlxe8wgjpg76vdcma8p,0x5C220801805DE63AC7B3CC64337E5ADAA2F9EB6954E89DEC327A8BBEB8BCD3E57DCA1A
                    182,validator_rdx1s0950rhwkpngk98q8rgm2lm83kglusz4ne2cn93aj8wt37awe0wg5w,0x5C220801805DEBAF083C21248D5F1122EDF2641CBCC43D0012138F7215D8F9A34F0643
                    183,validator_rdx1sv7j6tdzcuv8z7zg4md57ma6zre75xk2yv3el04c0vvvgx38knd6qn,0x5C220801805DEA55343FFB075A94EFEFA93AB19A90FE1D6C473481755082F010F901DA
                    184,validator_rdx1s0k2g57gxld3nydl8535e2tkp6tduh6pmynd3mf2a3q428zh09n24s,0x5C220801805D9C54024FB7E3223C9A83E4981E35A9909A6BEAC4FF0B79C80F75B1A582
                    185,validator_rdx1svpqafv4te3qtfhux5yxv2vyv95mp866dp9xpe28v0pvvwzhrk4vyz,0x5C220801805D9C3340FDB7A7D18BED05725CBCCD83C69B36658E819AEC93564F1E0589
                    186,validator_rdx1swghradxke9a3gt5ewddnc38fyj6mllhzvphyf3nen26usqh85m8zs,0x5C220801805DE50E6072CDBABA30BE80DBD1A8AFE5F877420B3F10283F4F9E47835281
                    187,validator_rdx1sw82mec54qgszlp7ycrfqr2hkmw33sy9chv0a7urrv8lcm2gvv9ynj,0x5C220801805D4B5C374DBE1E2C73534C877FC64A1FCED98CF04750C4E4D7010916CAB7
                    188,validator_rdx1s0quzpxcnvh2h7ua2rq6ds0nka3z05kfpz3eventwpamve22uxjdxj,0x5C220801805D71387E74B1C171480873BC0AD545B267304F95E1A30C35E02DCA3D0542
                    189,validator_rdx1s05y7ey4904x6hpk667uxlgeq30vv5jcufdu026z6fxyfmn835rn64,0x5C220801805D07369E84674077ED312AA746A35DBE7B2A9522C3392AB44AC568DF08B9
                    190,validator_rdx1sdmsjj0acqmux07w3xmssc8exn9pz0gzlwszj9ps9r44e67h2ksl93,0x5C220801805D79F5AA7301170BE1790D88229AEAFF2913D909A90C9107B7F03CD74AC1
                    191,validator_rdx1sv2rav2zqvufxs9dk5yu92t00ncrcw3dfu64pxxz29vvq3tj4fem2p,0x5C220801805D90288807C16EB0B50DF79482606C8E579CD13BF1347A43813AC52B9F95
                    192,validator_rdx1sdpykcee9uq3wfwpwmwkylsc7gpn8kzhp2uhhyk3erpesyqmkt4x25,0x5C220801805DE47391B0624B47748CA9B361143697E9625AB2696D9CFEA683E76C2071
                    193,validator_rdx1sdgghcae054twgtq9rut8y5k45ged4u9ejgl6g92rvc6nkaztsznkw,0x5C220801805D281BB06409F13751FD2A1B44E8628346AAADAD739B2C8DDA94AC486FC4
                    194,validator_rdx1s0rtmgcdg0y05wr52c08am2dunql4tggwqrkws2597cu6mrdlput3u,0x5C220801805D75B7229002F280E6D06AA4E4BBF71419D5BCC73CA36CAC4FFBD2C07EC3
                    195,validator_rdx1svt2sqyme20n8xw54sds6pdsees8dj2msn0gusracy33ah7v0c084m,0x5C220801805DA589647D590D993F3177033281F7A04EB3E79A1D2432EEB064EE0E5DE0
                    196,validator_rdx1svfawmdwc77092hzhd6rzlay0tg4g8dw2sd37mu7skaqjlsq624rfa,0x5C220801805DC92C8222994719438372CFD803CA68E889C58731964E249AEDEBD9AC93
                    197,validator_rdx1sdcmd3ymwzvswgyva8lpknqrzuzzmmkac9my4auk29j5feumfh77fs,0x5C220801805DB4143A5A58B19C8434E438A0A6573627381221E4D3CF77865ED22DB207
                    198,validator_rdx1sv6q2t0943gcyudump48t7vv7fmjdt2fzmx2w9ytyxdmtekwz9huwt,0x5C220801805D3197E4FE936C7554BA847DAFBF2B04FAE3332AEA62BAE4F45A841C891B
                    199,validator_rdx1swqja2wqgg65ynqydnt62z6rutzjnzhnu5yzkf7xtf27p90nfdnvht,0x5C220801805D34CB318101FA081F3896D3466E57DE408237FCB923F2F4D4697ED43DC8
                    200,validator_rdx1sweyknyzw9lzh6hdjeq2avh9gg5g6l7dd8he77khnv76ac8ut84hq4,0x5C220801805DC05A1B96370A4C8EAEEC2F024326424AA3F6D9D6E19621F0C03B3D9BBE
                    201,validator_rdx1s002geu08u94z4unkjvhlvyhs0dzj0glq82t6mhhvmahh3w0gerqyh,0x5C220801805D2C91B7AA97715C92BF9B5EFA1AD782A8EF71E9F6C9D8447268DF51CD68
                    202,validator_rdx1swu8qvl3jqrs2t8lzzjncu7cku3cx4lccy6jumdz27hf4qehwxdk08,0x5C220801805DA78C42450FE84D6BC6FFEBA6C0E5551C30A161A021209BD6BC324BC776
                    203,validator_rdx1sv456cf0qzcey324stgxgpusjtewnf23zujj2ds0w4aq4fre2l8x3s,0x5C220801805DB11080CBB0066C5DB3D8FAC615A8643673A5B092B3AC5C23C42EFB8182
                    204,validator_rdx1s0txh4cx4nd7eh5nsxyt7h7njja5galwj602x57e9r2dszv6h2aahj,0x5C220801805D1BC25EBFE5DA0EA674197EEBFA1A946B83717ED7C0E7CB514C4F29E334
                    205,validator_rdx1swzcs3tgr8g9qlkyhkyxjtveeta0un4ayf99l76mtsgj5f3h332fq0,0x5C220801805D492BF79E1F6608CCA707ED84FBEAE12A0BE6ECAF0A197DB9CC27F9E934
                    206,validator_rdx1svjkwg05p7pgg35vnutje5vsd30440fur7hstp4hfguedjrnrmzp2w,0x5C220801805DC98C0D904D2F4E34037D8C78BEF2351FDA96C497ED472B390A422933E9
                    207,validator_rdx1sw4ekx00nfjddp32vptgp48kqnr5y7ca4zsg82zchjasujya7yy6dz,0x5C220801805D63295217A12EFCD4B121338A41E6BC8C68C029D2C82D1BAC0BF58796B5
                    208,validator_rdx1sdjy58guswld3pajj7kc9etmjxx4r6fpn9kg9qvmtyuktl86m6ksek,0x5C220801805DBBBEED0F10F00F7D2400DFAA738FE70F03FCB2813AA2EAD99614C21EB8
                    209,validator_rdx1svn77rj5gmhhhapafxhd3tv9yg9eq2a2gc578weeqx6huurlfu5aec,0x5C220801805DCDAA94A6289AD6295D4B32B45C3A138522522FA281B72F0D750497B663
                    210,validator_rdx1sd0y2dhn2n59e3u4mv0c79c90rz37wek9lvddwpzfd6uadsuvaepag,0x5C220801805DEC435B514521A5A3603EA70DCAFEEA9F5FC152C0AA8C1926E66858B5C7
                    211,validator_rdx1svm98ppmxzft82lykaeudwp3z562dlqk692ws5us5a8lr78ux3v8kr,0x5C220801805DBAC0BC8DBC5D4FCC1CCF2ECBD862EE7150FB667B599C328074F0731F84
                    212,validator_rdx1sw0wxsnz26yazz0wyjp7a4xp3ldjwcusl57sa0uupmpc7j2266s5tp,0x5C220801805DBED370AF25B8E01238321E6442FB9AA40E5C41A58885F4987479E7EBD5
                    213,validator_rdx1sd2ldt5lcgry4uqtr8hzwq9x94kdskwgl77w0a5jkwq0fayg6k85wy,0x5C220801805D87F12280B83BA2DC73AB0AE1DECCBCFE1CC259CE0D4C8A2F56E1D784CE
                    214,validator_rdx1sd0s8g47y6w37dezandh2re9d4h7v252hq5a2kwj0swk0s7kkc2dkt,0x5C220801805DE255877BDC69CFA98DFB11E36035E3749FAAC359D8E5BC14CB162CFB2C
                    215,validator_rdx1swud5a22cnwj9n3fgnkvmn9pvmk67qw6ukw23xkmau6dncm23jskup,0x5C220801805D3770A57ABDD0F0997A283FE80013015BBED2FEA32CDB4E8ACCB38A40B3
                    216,validator_rdx1svg8sfeacx9nqe0jkep44dyyumxg0hw2zzzszg0334qlqm2hx5r979,0x5C220801805D69C7F745148EB30BE9AC8F880F08E9567DDFEC0568D9B1994A708C0FE9
                    217,validator_rdx1sw9097kv3jjg6jt4epf0sz47vmrdd6gys0gu8sul5t6w55qf3j7zfq,0x5C220801805D777E97619C91903E990376DCDFFF4FB85EA10DBC605188CB34970C6F86
                    218,validator_rdx1swam763hnn67utwv8qhz8aupt2y3k25hfdaulq8cl7m9s8qtrgfkde,0x5C220801805DADAD6580F62B093EBF5369A2276393735850C76A5F4484B562ED9ED646
                    219,validator_rdx1swsayvu3v3a5fp2n8zsg9awm6n2mshjj78gtl8s0kphs4s6r3hy0nx,0x5C220801805DB5A6E9C1A83830B0333B6F6E130AE55FBE323E4C003C2A0B2EC6B7AB61
                    220,validator_rdx1sv559zk2z927wmc9464kypyjltjrhelcmrx8486x37sv6xartj5x8h,0x5C220801805D4EFE9A0648C30D6971C38E5DAF90DC2081CD2A22940D31DE79722AE381
                    221,validator_rdx1svvvd0aclap95f7sm2hsyd6j8xq8amchs7752jan84vpf2vqg4rkpx,0x5C220801805D4D517EE659C96C8D7D98158F41583E81531CB5C18A9E23A1A2BE7D931A
                    222,validator_rdx1s0nmk0qu3sk9nsq3yxdfn06cwfv0x6vd0zmvch0m6634d6pp2lf770,0x5C220801805DAE9ABCC44E0290940B95B3B92F3827550167B87C8D8194127DA933E927
                    223,validator_rdx1s0g3xkjh9gs2g9xutwuuexmgta7kmmunwlk6pxvr978z89axrzv6d9,0x5C220801805D747B8D5AC40EA0DD14B1D7C5E296421D60478DF8A0039E90ED2F6EEE9B
                    224,validator_rdx1s04djmd2xrgc4z6ntwhpgd72fkmav2x2s5nvpzqc5904xyx9r7e3jv,0x5C220801805D249E5D72EAC159C9E252EBE018E7EC74858A136B6CECE0BC9D07EA5266
                    225,validator_rdx1s0yv2n460xdkefxlqrze4afgclfsq4akwz9em8nshjcga8ntemmkga,0x5C220801805DC4A3866EBB6F9547B8A0F9248235DDFE772B6595A8402D3705F86803EC
                    226,validator_rdx1sdtw97svylwsz8hy7y2xqnqflk8scayy9x04eapqpuejndlpwe6neg,0x5C220801805DB19A3CC41F7463D83584A65C8FB1C3E152BD8F1143F0AC2BE6BBE6CCEC
                    227,validator_rdx1s0kenl8rwh8e0qxpfrxhw7ry3uzl5x7gsu2ur7jfg6ec4sluk5gv05,0x5C220801805DA867A4352CE355C3E186D981AD3A9396F930A72999298C54682598C131
                    228,validator_rdx1sde3jp4g6ldh2mg4af0v0sq3c8en4agk3p7p63zcv58l5h05sej4kh,0x5C220801805D030A9755380D93566585D9FCCD5607AE7050177887E8E8F4E19FD23F7E
                    229,validator_rdx1s07wktx5t04rpq377dwtt0zuay63xc3g6z8at087dxetxtmcgzwqh0,0x5C220801805DC21B5A35BACFD544CE6E34A3BDC0923827B8EF4CA26D93F505E91326D6
                    230,validator_rdx1swfshw753370kp7ptc60ne4y9da549dvc243z08cgpv3ttvdyry0g9,0x5C220801805DCD16995B75E373A767AEF6C594BC24AE77A642194A3CF4AC12D4977FF3
                    1000,validator_rdx1swslug7tu9rgww8zdd0x8htptzgw92vx9606lx2ptdm9wsdam8uvxq,0x5C220801805D2959EBEF7A54F38F2892CA692BAC7E6670F32024BED0355CD2DAC58F1F
                    1001,validator_rdx1s08twr4tpfkxy5sy5e4yaz38uhgdcu3gjyzuexaheqq6mqxsjx7440,0x5C220801805DCB6ECDAFFED00418C4ADA801B11F7AB373E0B842CEF5A7C13DFBDDF4D0
                    1002,validator_rdx1s0vujyj9z05ykwe7v5y8pet52y8syh6dahzad0ttc565e9dvnzsm34,0x5C220801805D93D14981F51F53AE2943A9580F1714A86B5A93CA2B8DA91C8E2438A40E
                    1003,validator_rdx1sdnqgc9crzvzyq9mnnqhyg7st869r5td68wf8zq0mxkxeswwctd4ll,0x5C220801805D09FF662D3A7B8D0182478BA71D87492E1A5B4B0207E044D5E94DA30085
                    1004,validator_rdx1s094me32wep9zfr0dpc9muhna7ar4lg8ch6vl44m7cr04p50kkmayv,0x5C220801805DE9101662AA343DF38D8AD35A5737B87A6DBA449E4F3F2FCD3E83A9CC59
                    1005,validator_rdx1sdcheqrngr92u8mkd7pvu57grys54q3mkl4cvcc7nj92kk3n56up6s,0x5C220801805D5E2DE6C18077B3CABAA0D7063F19F22F8C167BEC8D6B5A2637C5F885E1
                    1006,validator_rdx1svenyxdrq8s348pmfjj6lk7d43l32e2rsh47q6qzx5a0gxze3m8q5h,0x5C220801805D69B7B5D71F43D850F07D4AB7B8DC8697639057345F18FAA62357EE3C1C
                    1007,validator_rdx1svfvxj0glmg0ea2m0nqm47qlfypj2rcmp74t7p4wgjx2m0f2nju2zz,0x5C220801805D0C582A1A2CA149DCE78D5B26F1BE054DEA8C06E87CA6950E8E50C87A21
                    1008,validator_rdx1sd5368vqdmjk0y2w7ymdts02cz9c52858gpyny56xdvzuheepdeyy0,0x5C220801805D76851BDCD01C874E98DCDCBA7C4CD8853BF05238BE12A701DF8BAA1326
                    1009,validator_rdx1swtv0s6ryf8rdce5zkml450ch23rzww9ajquz3p4lr3e87asq5sa94,0x5C220801805DB2742C6FEDC6627FC4F0869F498F38EA45362AF8311F13B948F555CB3F
                    1010,validator_rdx1swqujannq4qknqztw7j2gynuyqxs63ktm9p9d78gvsuwhr9wy8uvxn,0x5C220801805DC3F5D2EB755CA1230690FE340E8899F35032712C87BE7879EE8C86E397
                    1011,validator_rdx1s0y90ud7edmnaz7x52s7xsyhf8rgu2xr75u9526zqznhc5w3hkt34u,0x5C220801805D69A849233DB72619420A9CB406483E144F654ABFB00D72C636D980F202
                    1012,validator_rdx1s0tkjt8mureaw3kkfl3900law5tnrlg3wte9f4vlaruxe4nfjzdam7,0x5C220801805D1A743349DB9957C079E102E1BAEADEED31D589F25AB20F691FB8196E18
                    1013,validator_rdx1s0eas2h6lrx3a68d5vv4afetaql49tsanjkjq9d6gscx4ea7jk982a,0x5C220801805D26DCC84BEA2988332C126D527BFF634922C1DB06AED370C8D9CBDD3B41
                    1014,validator_rdx1sdep83pjpyhsctth6qc57xyyhw637p09dy2mrstmp0d0j74yzq96gh,0x5C220801805D187F291FD8E98565E1CCC1E9293B0CE12A0898E66907257AD89ED46436
                    1015,validator_rdx1s0lce50yzwnz0lgdn0hl3p4c4uxhnsla8qv5da04v6m6v2dtmuhd6z,0x5C220801805DE16A065E588214C75C94FD1E910C293DB331544E456BF95DF72F6AB781
                    1016,validator_rdx1s089c3309u3celwxur3zrz5t8skypzcff92qezlec9maawu8d5r0na,0x5C220801805D0534B84E9969121C560774BC7C56310075D7922A8749A9D07CD2AE8748
                    1017,validator_rdx1sdft8h765zwlfcc78la0lhng83gfvwst6waxd5h86kungj8lw8pdd8,0x5C220801805D079B3AB88C7C4FCF57E3A83A1D9B418FA75EB1CA96CF7C6C097F7FAB4B
                    1018,validator_rdx1s0lzdl35qkd0tmf2xp828wpma97fq94t4u536jpss68dnlr9kw69gg,0x5C220801805D9B825467841FB106D28D604535B8CFD60442BAFB95AAD34E2A5972F02A
                    1019,validator_rdx1s007thnssqwg0pa332ynxmyvremuvv9uuggnmm6y6qgj0nez5fjnx9,0x5C220801805D5C7A2B4CE4AAF0D40C8FA977A0BB1F689F4F72AC921D0F91A365FBC0C7
                    1020,validator_rdx1swqzqfll4r6h45e6rxx7p45vk9nn74mk0zlts4r3td2fj85kaekq0t,0x5C220801805DE7188E08BD04EFA5D0FC5E4EA0B7EA5184432561C6EF5BEEF83E70C0CA
                    1021,validator_rdx1sv77tce6xvprl6yg954yq78kshrqx39pq78unhy4jj9qv680uz0rdg,0x5C220801805D6D1AD96F84EAD2CFBB4559B32D3E112B96467868C0431DFCE0AA4E5EFF
                    1022,validator_rdx1sd7vupu267ef8kxvshd3v23xwkavt8kv8q3qegnwn6n88l26y5t09a,0x5C220801805D332CB29E6C63589878C9109D85D510EB3B9CB23164333FE087FF7AC4AA
                    1023,validator_rdx1s030jqqp9d9z55t234g53ts5zqmqn7xuwj8hyqtnhfqxgkg4wtu2n5,0x5C220801805DD2A17E786EE3EC9031ABB463C4690886E81C64C98B2E5A8DA4852099B9
                    1024,validator_rdx1svudtxxkegaeg6ks0qgjujfp8g80de2f63ygvfqug6zv987e99jk3q,0x5C220801805DEF42AE11072CEC22C1896120D5B1A617753DA871024C3F42347A629F7C
                    1025,validator_rdx1s0rvwhcxsgm9vmlne7exkcdm3x5pukqpze9wgsq8dhzpj6dcrdyyfv,0x5C220801805D9322285BBDF62DA33A999EA46A5DE389EDDD9C9D3965D6AF2DD7517052
                    1026,validator_rdx1sdzyh7reza3k7y9cyu93ghnak2n89uhugwc072kxrl7unsxgsacx8j,0x5C220801805D04844F1855A80C015A1FDC5F9523F3AEDE8D5F2917C438854419B8375D
                    1027,validator_rdx1sdha45dw0sj2z8c3qdrzua237nycpmdrqfnn6hcrg4hquyhlm9yu9q,0x5C220801805D80582123A5C815EBE403010649F94151DD4CA0A514CF3E757AF8CD6ECD
                    1028,validator_rdx1swh0w4fyck29xfpq3e422jlvgmltrl4g5mh3l3j99s3lf4gz4tywk5,0x5C220801805D2E3EADCECA5939AE2FB2BAB850D8D27588E8CEA2779B64A47C53F495F1
                    1029,validator_rdx1s0883tdmfmeekdks2v8uzwpqqlqh56zy95zu6svge4hksafyqmew54,0x5C220801805DDE72E6E98D6600EE3FE09288C24183C52B73F85C704E8AE50FF016DF75
                    1030,validator_rdx1swz29wl6rvd34w34rjnnd6x0dwkc8hltc9lhr8rl8g6876egxd5s6e,0x5C220801805D096E8156D4BABA4968191C02097EC10D8F012DC3EA4545F4510A1198A0
                    1031,validator_rdx1swqnvclxd9j8z937zr9y4hvjecex7235dz00xezejtuc3wg7j2rxrh,0x5C220801805DB5B39BF90D72705933BC7E8FEBEA9811A84D28F5B63756E696E397870C
                    1032,validator_rdx1sw5rrhkxs65kl9xcxu7t9yu3k8ptscjwamum4phclk297j6r28g8kd,0x5C220801805D81F1724CDA9FEF0C7C44A075377223B63D76F44224E3E68CCB01AEE893
                    1033,validator_rdx1s0uxpf94tg6ufuc57se490z842uynen04kg8rghe973gxex3l9wzrn,0x5C220801805D7516FF4E70CBA45570C6CCD1016C4F6DE56445E5BC32AEA114628E7E0A
                    1034,validator_rdx1sva6pmkgm5yacumw4p6k0xsfnqg598xkj9p4e2a58dl6gcrqpx7z86,0x5C220801805D43FA17D9F7C3A4FDBCBB63DD2C259400E53048248B232EC7245BD77374
                    1035,validator_rdx1swkwr2s9f33wlr8tejnum0j7edyswua6yyakg2tht6ns278h40l9qu,0x5C220801805DCEFD2E610B0C73483231785B19D4E2613EE7CDC5CE53F54FEE1AD0E462
                    1036,validator_rdx1sdwuys6rx7f5n2qy8kplkywq8ryyw5uj7rsac22g29lt53jzvs9kp3,0x5C220801805D39C5CC3D437F2385EB6A963F63D6D329D4C1CC042DA2CC82A747E8AA87
                    1037,validator_rdx1s07vx42l8ku9rplaw2apd5dzfgkcyn0s9zalu7pe77cm7rvee0m8ck,0x5C220801805D6EBB2A21AFCB884A5941F658AB2234AE5E7A3876B418238068ADEE0377
                    1038,validator_rdx1s07tu4u9kleletlv29hsmldrw5yejnznp7qe9h738gjaj2j9vu9pcf,0x5C220801805D598D52D1E9ED35C1D2752952CC920BF098D8E24109FD0250AE5E9AA2C4
                    1039,validator_rdx1s09c7w6220ccgz8vcjkkwnwcuqrp9gh6xj2ksvvck537eztg4texc0,0x5C220801805D48C2CA91B19DFCA35A4AE0E33DC4892B57CAB3815A4708C8243FDA92D4
                    1040,validator_rdx1sv856az023f380u0gzezaf50djcvcmpzf4ntv5au9xzt68sqjra7n3,0x5C220801805DD7B868CCF69076667EAFB75D4E7FB6C5B7BC84AE2D9B89F4F328E1DF68
                    1041,validator_rdx1sw2qt9k0placmjgwku2kvk66c2v8v0f9ny2fkvzhsh7wwzey6g7zam,0x5C220801805D7ACB4D8E8AAEBF920310AD22582097A0240DB74013E6976C0A8E35FA3A
                    1042,validator_rdx1sw64yy3mmk9xvy6ys3gn7fjl0khckjzjkdqs5vqjsdtx6e3gtu3pcx,0x5C220801805DF2466403F7B45010A62366178DA6E83F7C4C6DB78A60FEE148C31DFDB5
                    1043,validator_rdx1svlvyezsxp467pznd2kfnhqpenmw7hpdgnxjgu3s8mzxtruz77d3q3,0x5C220801805DCE3977A19E5F4895E4888D889932794BAFD9262DC5B6542FF466E5C1BA
                    1044,validator_rdx1sddlrsqfxvk7xp2tufkhzdkqaywdv6mwkdd3ajfaql33aumzdm374x,0x5C220801805D084F76A6B518C7A8AE59A4304863BC031D1243C52E7A70A9AB7965A42E
                    1045,validator_rdx1svlpfx0kxp5yk7h8dqrd8uzqtm7dkd0mlmerq6xsy3rng84d4wqnmv,0x5C220801805D37E1CA341F0CD5C8144DA295ECA4D1452C04379599D675F3B2A32A0348
                    1046,validator_rdx1sw3vpmmyykrc9vk8xp9hk4595d0qv9q8pmm2pkursum65r67gsrwwq,0x5C220801805DC4B042C30872C0BB9482A386C854D9F7D38EBA54AA4C792A86BB2F3130
                    1047,validator_rdx1swj6hla4w240zf3p3967eu0yvvh4ypzt82tp5aqxh5637psms5wdq2,0x5C220801805D2CF4233D2F06B217A6DCEAD60DC87184B7C2535C72611660A0940257F8
                    1048,validator_rdx1s04r2c2pcq7mhs7s4pfy4a22xemes7wquuwmfsxdpfmw73zvp9gkha,0x5C220801805DB499FC17B239860E4B409A4F12920F2E941281DCEA1C15E2F0D8774890
                    1049,validator_rdx1sdd0s7vy83qtqevqj7dgkuf660hfmf6xn4dl6t8rasg8njwh4nrx55,0x5C220801805D020259B4C8157CEB8A49896A715018695C8F3F9206FA025D9A15FFB4AC
                    1050,validator_rdx1swq6xdqxrg40k99lk08fdrd6tlnnzmn9a0j7s3s0n7pu2etrmt7yvx,0x5C220801805D0E0F61B62527FAF9E02885E0359E4AE352CD74DB2B805BE0A11B2E96E1
                    1051,validator_rdx1sw8387nsvja2lu0n739ar3uhz5kmcxd968rc5h7z0tr4nx3e8a2zkp,0x5C220801805DF38211D680445020BD7FD26598FF93CECA3793F6F1A388442A97E732F6
                    1052,validator_rdx1s0vn7ay93r07gc3f45efqmjz03t797wa7980v2rp864c78zp3slerg,0x5C220801805DE9D289E030488FFCCB64A43F7A9146A26A1A599C6FFA7AA4F24AC76207
                    1053,validator_rdx1sw2l08wcpyhhta0f9wmd3py8yjlrepkdxxz65nhqjq2u5939z728fj,0x5C220801805DA267A2EE212EDA917EFD4CFCE4F6808D66D95946C9639B1EC44A6567FB
                    1054,validator_rdx1swu0wz9w2ppkhpqelt358pjmctr8lhlmwd7e3ggpw6nry29jzu3van,0x5C220801805D4400F5A5E0822B1661E4CE8875F2D34FD56CD51094E9BEB92DAB1C9439
                    1055,validator_rdx1sddrqeazndkjra8e8zaxy7whecgjvfvyu4kd25rxhxm7lgscsget65,0x5C220801805D88B330C76B9EE1EE8481257B0108133CEB01FD3FD834ED92544691837F
                    1056,validator_rdx1sv3rkch3kj85uwk9yp37v2a68alnmm8aqvd3mefswcvn94ch2l2nnj,0x5C220801805DEADD2D021EA2B40A25C8D8CEB0FEF4F45848CCBB0FC6F571F09F5B928A
                    1057,validator_rdx1svjhajkrvar9lc4q045t5n02llhdm95wx2pampdm9tc3fglxdgjc8a,0x5C220801805DE76C8FA07FF966FEF89295A06520DB1A098277CA04C3070E542180D41A
                    1058,validator_rdx1swz0rnaagve0y4wjk00dedlgztqyn39nw5qrtv0xkwd7krgfa98uwt,0x5C220801805D97A7AD78EBEB58CEF37BAAE1331094322FA7AD3488CB0AF5FCFF733F8F
                    1059,validator_rdx1swec483k6fg9ntrvlcfu549da8sarywnq8wacdm6646vmujjp8mrgk,0x5C220801805D13366EA4D159289E84C28FB81ED1F09EF1A78DB839D2EB0A792F4F18CE
                    1060,validator_rdx1sva9supntr9l2r6mfw46puhmmmdvlc7ecspcryqq3crj8fcf4jrt8t,0x5C220801805D635F82D033834902918A4FC355775187E34D0B3EA582EABC466DD5BE9C
                    1061,validator_rdx1sdtezkpzjkpa8cwm2hurg5quz3unaxdlj2alydc0xns49az0pd9tay,0x5C220801805D2AB9147C7F110AA5EE670FBA6BB25646D3E16A4639D0308C4C2F961B1A
                    1062,validator_rdx1svfjk4s7mx0pj850up74egz53vcylw8s2jaxzlgjzs5ed5nmj9f65y,0x5C220801805D9119A705F00C681C834B9EB87655171B954709EC63BC079BA0AEC61B0D
                    1063,validator_rdx1s08nr8ukas7yklpf7q0dg9s3resvmhjau9r5ye3vvkrhpycayjrfrw,0x5C220801805D1AF0E84EC3078FD4CC9699D9EC72C2B14D650CFD6CE00A0C04597185D2
                    1064,validator_rdx1sd4eq4vvnrmtxy0l4wxaykugwjmyflnnkn4sz3p9jv79ac2sv5sh88,0x5C220801805D005FECEF3929BBACA86ED10C309E17CCA5819098876D3B70E61BCDBBE0
                    1065,validator_rdx1s078ehp7tdedmvejsxa0efzy0f8pwdkgf7x59cadkqgr0ll8px433n,0x5C220801805DB7A0A7B82A9834820E3CA9C5CEA3AE4C22345F1EF91C1F225FD94FA1E3
                    1066,validator_rdx1svstwumv0nag4p9wmefdhfgrgpx8tc57w35q9ae39yhz02rv88ttjp,0x5C220801805DCB4A6AB3844BAC61F8B818A182308A3BD1C09D3E36D966DBCE5561D2D7
                    1067,validator_rdx1sd34p0xjh6987ly6qtmkglhwucm4wpx3ykfj9y44jxr9d0mh2ws24g,0x5C220801805D51EE75C00C37D58DD5784B7329E7B75D00B41313DB20256966A14098B6
                    1068,validator_rdx1s066xuq885l0mttgmx4ptflte6fepkt0c06mqnqtdgajj4mcwh70q4,0x5C220801805D76ECDFCDBE5288F6D90CF94C7063120F021C1E4C836CF12C6CE163D2C2
                    1069,validator_rdx1svflx5cpjjys8pl24enflstfx2d887semg0k6yl4x84g8qvqx6etkr,0x5C220801805DDE0840EE5A9A752D57876DC89AF2656F32A64902ABD1C9E2A164661E25
                    1070,validator_rdx1sdt7m2m3umwyuk9evzzcg6q34s5elu64pjkxlhtqu2w4zh65c99sw0,0x5C220801805DCDCA46D8297A4A518A9E43BF511F31F479DADB80AA0F5FC11A085D4E28
                    1071,validator_rdx1svw2q8uz076euum7yvfhqqxfym5tfjwskkfzrtcz5wpwvdkes3xz42,0x5C220801805D283555645FD0579A8D78AA880C28A2D55D469F202627897F44724AEDA2
                    1072,validator_rdx1sdvntpsfvlyx2hapn5zfr6z7etfwgqljsqdqh23876r33fpd8cvu5j,0x5C220801805DF84A495104A16D8240579EFEED4ED4493AF9BD0FD8CBCDDACE33123D49
                    1073,validator_rdx1sw5va0gazh39jypat3t55vjskc90v93c558ef9c6pjkrctx8quzh6d,0x5C220801805DE3A423D778C0CE44C2B1EDCDBE5DB86BBDF23E1575ECD0954107DAA3A5
                    1074,validator_rdx1swkwafkwtq2dycr8av6zt5zlu3ls92r7vu66vu6lep6me8dphesxlj,0x5C220801805D95F680EE517620616212BB5E3EB9026C40C06A4FEB6ABF9C4816ACA5B9
                    1075,validator_rdx1svr5kd8p448kqqmffrg5nztmgdedpns68lfm2382p9l59jjcwvkd20,0x5C220801805DB1AD14A274D300EE24B4D218C622488E33C21B12AF043F360F27B59781
                    1076,validator_rdx1sw22dt4lrvdulpyx6yvxqwfzxklmx9sqzz64tyau0drrxjvx8ysa7u,0x5C220801805D77619CBEB59B313BAC26509D22DFB9B94710D66043E027D1BD41E925B8
                    1077,validator_rdx1sd9xdzed3se3xn70gv2fvcf6tfgmrv2vn9fymcx2p7c2mr8gaz9qng,0x5C220801805DE580F89DDE458D12E78497677E47F159A3071420616A5985A8DAD57130
                    1078,validator_rdx1s0sh920quwrqqrhev5yzm6yqxzmn09gcf6cdr3y8yvn2ws6chdha53,0x5C220801805DE30B838ED6494AE2DCC3907771531EC179426B30CDC6E4413C82F43AE3
                    1079,validator_rdx1s0g5uuw3a7ad7akueetzq5lpejzp9uw5glv2qnflvymgendvepgduj,0x5C220801805D9261C1296FEE752646938281A068C6F1EC272F810A5C799BD2213E4B3F
                    1080,validator_rdx1sdy4cz4jp2jzrzu37rlmr8sdenevgtly7p0wx5erc5sfkh65c4j0c3,0x5C220801805D101868807CAD13B6DD1FAFE30481B182A6C81ED2529B651D6EA0A907A1
                    1081,validator_rdx1s0ylcs2ra780e45n6n605ujr52aemv6equgqr6lwycph92rmgpwxme,0x5C220801805D2E83905CB0990BBED33A0302054B1FC6B9BFB3057D2DD898A8111F8148
                    1082,validator_rdx1sdu0t3wrxf2vzz96a0mxuzen50tk65jqv2n6eg08etw22477ew5e88,0x5C220801805D7BF214A653468C89EEAA95E3E5A91226990357943F17B18EACE9896219
                    1083,validator_rdx1s0jk8ccns3ngkc29wpq73zvmjmt0ln2lnl8yfwfqccsrhw4rwsvdjq,0x5C220801805D529D2A7E3F15B9D91D8E4073B10EEC057F4AAD5917BC3B9824D760A674
                    1084,validator_rdx1sdf4sd9pwnr9q9dgd2rr0a3ujz6y2qpukyg7x3gjemvx6ht8fnukqx,0x5C220801805D0DD2ED394090F91682D63DEB757EFADDF37837601CC51DF1EC8D77F19A
                    1085,validator_rdx1svvnmmgdr6sl7llxc8sdu32ys7tn8v6k8quh7e6m6ec83fr3pf6d04,0x5C220801805DE3D6D8B1EFF3199E6219028BAEDD8605B0AEAA07760F1A8D21FF0E478E
                    1086,validator_rdx1s0phwevr0tcenaaptx4ecmduw8zy8yv449vzmktw2wk2x8r47vfnf0,0x5C220801805D8B83A2CB350A12F08D0914237B7F770D4DBFE5AF15FB3F816909D20663
                    1087,validator_rdx1svheuuvqlmp4l2mhx99m7xldk8tk2lu8kfjgdvxaes9xevpuww63se,0x5C220801805D1055680C9AB4BF23EBDFE310309284CD24209AD41F602C86864281D740
                    1088,validator_rdx1sv6wrgkqxhh9970jxy7smnlzwje2mdy0glkmw5xpjmvzg4wu78snz3,0x5C220801805DB98BEF3E390529A72F96C2DB91EE6F21C114475C09E41AA9FD6387CF78
                    1089,validator_rdx1sdzp792ktu30rd0kvj5stxu7mdpwylgkprdpnx5ysv2uj4km3ggrvf,0x5C220801805D7FDF9E87B5581FEAB6856DC46285120853BEA36B122AA9929CB5385358
                    1090,validator_rdx1sdvcfj2tcg8cjtqzymmqukdt2ue9qpqmfq8hl7zx00whdmspj0u55k,0x5C220801805DB4999A22C03799F7D3592CDF48FA19AEAEFFA093C33A3582EDB337A257
                    1091,validator_rdx1swffjvsu78ej866pnd89yhv5ragthk67whp68fyw9eudt4fsxjdua6,0x5C220801805D200E3F3B09D1FCECC397818372EB5ED24AD06FE3DA9C2392B8465FD36E
                    1092,validator_rdx1sw32mp374vrd0extsg4d6z3mwpgpalydnt5tp8a6fnsq0smax4tv35,0x5C220801805D0A63586CA5D16BC6BFC31DB6ACCC52375D1D08F8F11EBF4A6CE3061671
                    1093,validator_rdx1swcdj6s9u3sms92a7mrh3gm5ejcj5qnu7wes7frurqmlu44df9sye6,0x5C220801805D5D99D7117D1EF4F5A94DE44C48CB1834E016D9F96FA4B85830644AD01B
                    1094,validator_rdx1s08sa3xzeljmm6kcu020nkl2jzg09nt52agqwdjfj463fs6stzk90l,0x5C220801805D0649CA1789C412158FB4D4533BFAC0FC4A50679C0FCA9CA453B9A7ECDD
                    1095,validator_rdx1sdz3y8g574nuwnp4tcql7aysln4z2zml4g5x6cetvqn40jc9jvstv2,0x5C220801805D4D4370F77007F667489F554A7EA42901305EF0F0370851E3517C970AC6
                    1096,validator_rdx1sww07qz60wgkl0me26nhz5fz38psuwv27ddlfu5d876xhekfxxwhwd,0x5C220801805DB432BCAD1AAAC72F3CF159C5E0859554EA97208E9D7D07581CA83BABC0
                    1097,validator_rdx1sd2fdyrmnhrrr3gmrx47atpgh3nvhpax66aufcwj3ey9lyvyqgvndf,0x5C220801805D8BD7CF6843992A70BAE83CF700537C98E555CD9B16E7C21D4E964727B3
                    1098,validator_rdx1sw2hsa48r7vsut5cj4sahuzkxdgzwkj63t2xjjm5nl6c20hc7npt8m,0x5C220801805D7992C741C68ACF55AA621811038993B41AEBC9A5FE1DE431028E3A1FAC
                    1099,validator_rdx1s0ys67stq2qpk0gvhrrev9g8v58qdkuse7lzf4qgp8cz6pxyyzk2ur,0x5C220801805DDDC42850738F9AA1633AE93C4B15B39C77B43823D951009D144B3E01CD
                    1820,validator_rdx1s0lc0avj3hpkkunyw8vn69ayv76ruejj4myakdgn3t9nptdplmfshl,0x5C220801805DDD75A557803DA3EA4BDB8535BE4C21016B6B527F24597DAD629EAA6B5C
                    1821,validator_rdx1sw54cuswwzlcgw2zh3ax93pddnsm78qwwhmtvz650q84yyckzkh7nk,0x5C220801805D91A9B5DA2EDD2CD7E9E1622B45983124C9FE247F9AAB98D56198056567
                    1822,validator_rdx1svumk8xfrm7e2khc9fzy2a6m385ceh0s9aqg05ayfkhls56vw5r5ys,0x5C220801805D126554A39E23163A6863969720A3C67CD320D422EBB50BE26D0635214C
                    1823,validator_rdx1s0hfv69389tvaq09y4lttuhdfhfp488x5camz9gvmay4qvysj8mpad,0x5C220801805D84539F3D1C3EC6E1191E27E20EA0D3A9CD18402D46BDADA58DFEC35E75
                    1824,validator_rdx1sd6n65sx0thvfzfp6x0jp4qgwxtudpx575wpwqespdlva2wldul9xk,0x5C220801805DAE1DE64EF8267483E858327C99F808ADBFD977A79576B48FF223EED5F0
                    1825,validator_rdx1svlpy2y9lwxmrm5zlvuwkkrxwjzpmmlfk8n2nwmt78268pycqwntzl,0x5C220801805D725FB9F8070DDACF3F88DCC43B0590C2F6B2F74229B819D7FE2342CB37
                    1826,validator_rdx1s0jsc864dxv4da03nfhgpej89nj82v903eua49a9e9xk6ga5upk9ef,0x5C220801805D60FA4C7EDB960CCD98B0B9B9984E41152D72875D316F8333AB435E9CF4
                    1827,validator_rdx1svferc2k7gjm9gcnzffkml8qzyztq2d0g0jrjlegtrvnrnyv8zp0a6,0x5C220801805DD41C30F0FA4F86E4C93FFA74D860B8AC7B46FDEA41C9C66F4C20DE1C7D
                    1828,validator_rdx1s0qzv2vmxydpnglk36mczrdwczpsskuzek2cs5nnld6j533rzatmln,0x5C220801805D24444D0778652F631FFE3C314E7DC8C02C2B78CF375779869D1C73221E
                    1829,validator_rdx1s0wgthtlhvg9ztsjazlu9xxlm05hpphcuqaj9qaw3cf6l04n5s93gx,0x5C220801805D0507235981B8032AE2101F7877903091CF49B1204A93E5581C53C9AE62
                    1830,validator_rdx1svxx0jetjwnptndj60sm8h7ljs0v88fl6xhwcyp6ar397agwd0ezaz,0x5C220801805DED968FB3369BF5B9F5EBD514482325E40DD850BF9E7221F24A176DE305
                    1831,validator_rdx1sdw358qvf4k4mv6sfkl72c3tevsadqzxssaerzns6f3nrzxt5pu4t5,0x5C220801805DF7DF35D139D6D9096EE483D039F24354A0114DF685508D49116A5EEE54
                    1832,validator_rdx1sd3lant0vgk8jkxfgmmcd2zsljnu7xf2vu8lzc5cljgammdvfa9y7x,0x5C220801805DDEEEE481D9A227AFDF495B79E7A8ADAF597F5009D4529DA462B7C00FE5
                    1833,validator_rdx1sd5grh85as4y60mxtuupnv2e0036hyehszt6aa3285sqxdulh309jg,0x5C220801805D5C672BD07835B20307531A979CE31C1B23F81331F8D140112BFB95FA1B
                    1834,validator_rdx1svxqlmjy80l46uj3krg8mml65jsmfum95shvu20xjuekhq0mv2hw9q,0x5C220801805DC5773CDB1A71F4FFA096DB5087CB15CAC56A3AEC3806CE5D9D3C4D1804
                    1835,validator_rdx1s0jufe4lexzxuzyq7mgm2a9cfmfvxwqrxs3xeywg3fgmesrzn5nzrq,0x5C220801805D992F1BE4FA9A87F8D33854871C7C229CA61780FCCB552B2DC76B74F2CC
                    1836,validator_rdx1s07kn667akdcry7052ardf2r4lfnkv2k34g2ra5vfc3hwurud38qca,0x5C220801805D08079192D00DE9C70848D303D394CFA93175C5CBDF4E83B31DB21A3FDD
                    1837,validator_rdx1sdh43pkyltan8qucr0xh928mdkz3fgyeehajnqxxnhhz9ux8gp74k0,0x5C220801805D8E594309B8D8711E0ABD3D5FC915A56CEFAA0B33280698663C5DD857CF
                    1838,validator_rdx1sds4prpgf0p25pu458fg468nw9rtwqdawwg9w45hgf0t95yd3ncs09,0x5C220801805DA82423E70E0DCA50487E1D72E7A017A6B7BAEB33BAB959DA06C2CD835D
                    1839,validator_rdx1s0ey0put5afe7yfqyet8mz6f7mynfvdykqmr3kr663q6p0jc6yr7s2,0x5C220801805DC23C079C4FD5F8DE3EEB18B6B835E81D1D7B734427360FD0D36A06B2B2
                    1840,validator_rdx1sw8m09y4dmu29yaczh7rajxvwdfs5xwdfjw0x2ml7s9dmdh8w7vmx7,0x5C220801805D319A971551D48E1189A07C14C25F9574C8A3BF5593B0CDD0C61F0F8C3C
                    1841,validator_rdx1svhtry7r0xzp72kx35mlaq4uyth4cgnuv776dx4ld3f4xuuev2s4lu,0x5C220801805D7892237FC3490611F3A765B3FF27B33A60C4DBFC02B33CB9EB42EA0B5B
                    1842,validator_rdx1s0kpu7dq8nugrcvc6u3vnl9rhntzxyck0k3r5ze67r5knk4auxaq69,0x5C220801805D8C4A2D40B4E5CE4C5C1A4C3C6DCB8AFB8EA5F0D83C112F0C53B707310F
                    1843,validator_rdx1s048k34ctk3m57gumema2e5jmhfxhdryyr5hq42xa9q59pvn8lezg8,0x5C220801805DDCF16D29934DECFACBE38C090E24AEB51A32596116221ADE7ECDE3DADE
                    1844,validator_rdx1s0v38ep79xx6y7cv86afuqw3z2sd074sldcvrld4lefw3fagvjdhnm,0x5C220801805D050CC39A8D19847D56B137C7E9BAEFCBB345A879C2CDB4FB695C937BB8
                    1845,validator_rdx1sd8c8v9tffjtgqtuzygnctrmyfnhkd63avcgpzuknx3dlklds85rkv,0x5C220801805D410D29EC88CBC1435EAD73D857CC69A69896AFF5EF73AB0F2E18C92A00
                    1846,validator_rdx1swtf4pztzxs9sc6kgfq20egle474x6tergqxqh28vl9sqq8s8d6hrw,0x5C220801805DAAD89FB18B23819DA9427D43F10F3C98279479A94ED591A46F5B8935FF
                    1847,validator_rdx1swyv9xgmn7esfjnkewch6rqh0h6kxalkty6aer63mvqgt2qzg47ka7,0x5C220801805D7036B654863DAA697E88DB77EC269845D800C5AFE28484F6CFBD37EDFE
                    1848,validator_rdx1s0u9v6a4n2q7cvjkmgq0wnl8gml8c3purzdqg2cwdah2l4g086xzw4,0x5C220801805DE2D561410D2B874D4F6E054FC79079D42187BB045EE42F18B978888156
                    1849,validator_rdx1swlfjk9vkateur8y9y4r9kh8fnta507d4x39j6h0y3hgcge8svwupc,0x5C220801805D38790788828BF532D44F909712864A1164380222D1DA8EEF0728BEB883
                    1850,validator_rdx1s0rh0gz4mnszulvhrfmee5e9wcatwnyzldq43kp26r2qa0s45ac7ws,0x5C220801805D3E614AD85C9AAD77460DCEB1F3CAEC238531F29B05D615B6C8261FC594
                    1851,validator_rdx1s03yayle79ua6ez30mcc8nlf42rfjatmv62durjd9judn2zplxgu72,0x5C220801805D344579487F8E6FCAE78785CF3EB15953EA7B3F59E6ACE4856DA45F4652
                    1852,validator_rdx1sd9uhhpml8vjz8uz0tut9jzv2mx326jdusfxjpccccj2904s8ndhqq,0x5C220801805D3671D37271A59B1E460671B16B4836F6D17243D5A5BD2564F5A1436965
                    1853,validator_rdx1s0gcg276gq0uaa8mc88mk6umdejmyg5rmjsw0725fml825nmwpgwls,0x5C220801805DD3245A597E5A7BE4D52BAB2FFAC7BC3EB66162CC48858F2606EA29CC5F
                    1854,validator_rdx1s0eg9fzf0wm9yucpzug69fyngjpdw6w7ra50j74ktvahhhysmxyw68,0x5C220801805DDC43669FE70DD4C5FD56803FAD7A76C245820B87C1E1671866E8F228DA
                    1855,validator_rdx1s0nxs6d4k5fpw05paeal05sm8ffsgvvuf7jkcleucr3zjxf87wm5gc,0x5C220801805D3CF320CF5ED1D639B2C3B763FEBDE9CFCE9B18E8782DDD78C1DCB6C786
                    1856,validator_rdx1s0fsq205yuxukmpdrcaxqueq3phzknpvwahkskjvz69atk2c08f07d,0x5C220801805D9887FD374E56AC6F74BF294ECE497400A05CFFB7FFBB5E2D1079549EAA
                    1857,validator_rdx1sdf04wxuc7c4llwst8rw5sfj350gnlnluhrpy09wk2gwk5cmvgffpy,0x5C220801805DC62AA0E83C4EF57F9B1AC6D80EF26EAD4E1AE6BA2C37490F94FD4C30C9
                    1858,validator_rdx1s0er6tqwggdzr42gg3dn2sp3msyhu45y6dmssjfafl03daat33q036,0x5C220801805D65A799F8B4EE4E4E6925A6C1C770F0FFA92F6972B6C5B44A9030D84D7C
                    1859,validator_rdx1svsvx7w30pq4d2t3587r5tqkv7gne7pgdrtgtl94uefax07qr50quu,0x5C220801805DF8DB1C9D9164024111957B327FF115C393E19CB245A0426019D6D4CAEF
                    1860,validator_rdx1svq4qehhqedwwaerwdjyartk3p4u5mxkul49l9zqvdkj9utwhhfdw3,0x5C220801805D1177BF1B80E2A366C18A02117EC9E992EC2B5BB1C7F48D192F1B8C806C
                    1861,validator_rdx1s07zllgtzvy9xfyj34jfa9qpd004tcqg80c6vjezv5xmvk0d7jvcjm,0x5C220801805D5161AA0FE1E4346272FF11D7546715A96D45285864E7136ED80AE39816
                    1862,validator_rdx1swq5x8fman3xfccfwtl56asrnus8hg8khrn4zqkayvyamq5m96fc3d,0x5C220801805DFAB5BFF135E92C7AEF7DF469027E9A68C7D54329396F2B43D6BEE108D4
                    1863,validator_rdx1s0t8334zzf3dzrkpk4we48qfn9qqt2xkjwzx8u4pkwe0442pp3tenu,0x5C220801805DB8B9D05D9611E635F7C656C6C99B2E44EDCA1B6FDEA3E571634261397B
                    1864,validator_rdx1sd3m2lhf6ts8au09haxq69v7sg02fktpjn33w9krz0gvjadawp3y6x,0x5C220801805D930C499405C1F89349776793879DB59466E2A64EFDBFC70EB55BB872DF
                    1244305,validator_rdx1sdkgwxgqd0sf5mq4fc7rzf3z85rl93v4mazspa8vqju5a5c99av5zn,0x5C220801805D9148F2B6D4497B3ED1DF25BD96C86B94DD27D2A70CFE1AF2CDE264918B
                    1263086,validator_rdx1s0sr7xsr286jwffkkcwz8ffnkjlhc7h594xk5gvamtr8xqxr23a99a,0x5C220801805D9F6E73B7829B25CEAF582E71EEBAC8D0BA9CAC739AFCFF3F47577B393E
                    1306551,validator_rdx1s0qjxdwy5ssl9rnqquhv9gucpm6cvxvn8uwngw26c6y9ff06d6s3fy,0x5C220801805D6D9A076FDBD4047CF14C459BF315907773553E44A94E47FF54DA78D3A6
                    1315771,validator_rdx1sd73uhcf8v239nlghdqvs60zk34rmacdaqs9gh75nnneu53vzjtdyp,0x5C220801805DD9AB0D24802948CC8F8C16ACA3480E917332FF4EB888A05F52AD2F91C0
                    1325709,validator_rdx1sdslt8qu2e54as6gtuk4xzsyd0rr0yr4jqpr03eu86wk4mnsrw6gdv,0x5C220801805D7E8140189895BA85042C9BE078F8DDF3EBDD38D4469EC479E73D2D8DAB
                    """;

        var out1 = new List<string>();
        var out2 = new List<string>();
        var out3 = new List<string>();

        foreach (var line in input.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var cols = line.Split(',', StringSplitOptions.TrimEntries);
            var hex = Convert.FromHexString(cols[2][2..]);
            var addr = (MetadataGlobalAddressValue)ScryptoSborUtils.DecodeToGatewayMetadataItemValue(hex, 1);

            out1.Add(cols[0]);
            out2.Add("\"" + cols[1] + "\"");
            out3.Add("\"" + addr.Value + "\"");
        }

        var r1 = string.Join(",", out1);
        var r2 = string.Join(",", out2);
        var r3 = string.Join(",", out3);
    }

    [Fact]
    public void GivenSameValues_ShouldBeEqual()
    {
        var leftBytes = new byte[] { 1, 2, 3, 4, 5 };
        var rightBytes = new byte[] { 1, 2, 3, 4, 5 };
        var left = new ValueBytes(leftBytes);
        var right = new ValueBytes(rightBytes);

        leftBytes.Should().NotBeSameAs(rightBytes);
        left.Should().NotBeSameAs(right);
        left.Should().Be(right);
        left.Equals(right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
    }

    [Fact]
    public void GivenDifferentValues_ShouldNotBeEqual()
    {
        var leftBytes = new byte[] { 1, 2, 3, 4, 5 };
        var rightBytes = new byte[] { 1, 2, 3, 4, 6 };
        var left = new ValueBytes(leftBytes);
        var right = new ValueBytes(rightBytes);

        leftBytes.Should().NotBeSameAs(rightBytes);
        left.Should().NotBeSameAs(right);
        left.Should().NotBe(right);
        left.Equals(right).Should().BeFalse();
        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void GivenDefaultEqualityComparer_ShouldRespectValueEquality()
    {
        var hashSet = new HashSet<ValueBytes>
        {
            new(new byte[] { 1, 2, 3 }),
            new(new byte[] { 1, 2, 2 }),
            new(new byte[] { 1, 2, 2 }),
        };

        hashSet.Count.Should().Be(2);
    }

    [Fact]
    public void GivenNullArray_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueBytes(null!));
    }

    [Fact]
    public void Length_ShouldReturnArrayLength()
    {
        var valueBytes = new ValueBytes(new byte[] { 1, 2, 3, 4 });

        Assert.Equal(4, valueBytes.Length);
    }
}
