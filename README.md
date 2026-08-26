# 8 Slices of Pie — Checklist de Produção

**By The Sword** · Survival horror / stealth de loop · Top-down 2D · Unity 2022.3.62f3 (LTS) · 2D URP · PC (Windows)
Espelho do **GDD v1.6** (26/08/2026) · Entrega **31/10/2026**

> Marque `- [x]` só depois do item estar **na branch principal e testado em play mode**. Arquivo importado mas não usado na cena continua desmarcado.

**Índice:** [1 · Músicas e Áudio](#1--músicas-e-áudio) · [2 · Animações](#2--animações) · [3 · Texturas](#3--texturas) · [4 · Assets](#4--assets) · [5 · Scripts](#5--scripts)

---

# 1 · Músicas e Áudio

> Composição em **FL Studio** · export **WAV 16-bit / 44.1 kHz** · mixagem separada por camada (ambiente, Lobo, UI).

## 1.1 Trilha — faixas de fundo

- [ ] Tema de abertura / menu principal (loop)
- [ ] Faixa de fundo 1 — Ato I, a vila ainda viva (22:00–00:00)
- [ ] Faixa de fundo 2 — Ato II, a caçada começou (01:00–03:00)
- [ ] Faixa de fundo 3 — Ato III, a cidade de sangue (04:00–05:00)
- [ ] Faixa de fundo 4 — Ato IV, o Lobo sumiu / falso alívio (06:00)
- [ ] Regra do silêncio: intervalo real de silêncio entre uma faixa de fundo e outra
- [ ] Faixas de fundo curtas o bastante para não mascarar passo e respiração do Lobo

## 1.2 Trilha — faixas de tensão

- [ ] Tensão 1 — o Lobo está **farejando** o rastro (SUSPEITA)
- [ ] Tensão 2 — o Lobo está em **perseguição** (loop)
- [ ] Tensão 3 — janela de **bote**, o pico antes do ataque
- [ ] Stinger de entrada do Lobo no mapa (3ª fatia)
- [ ] Stinger da **cidade de sangue** (6ª fatia)
- [ ] Stinger da inversão da luz (7ª fatia)
- [ ] Faixa/queda de tensão ao escapar (RECUO ou fuga bem-sucedida)
- [ ] Crossfade entre fundo e tensão sem sobreposição audível
- [ ] Exclusão mútua garantida: nunca duas faixas simultâneas

## 1.3 Ambiência

- [ ] Ambiente geral da noite (vento, grilos, madeira estalando) — loop
- [ ] Ambiente da floresta (NO e SO)
- [ ] Ambiente do labirinto de sebes (folhagem, som abafado)
- [ ] Ambiente interno das casas íntegras (abafado, murmúrio de fundo)
- [ ] Ambiente interno das casas arruinadas (vento entrando pela parede rasgada)
- [ ] Ambiente da escola (corredor vazio, eco)
- [ ] Ambiente da cabana do caçador (moscas, gotejar)
- [ ] Ambiente da loja de conveniência (zumbido de lâmpada fluorescente)
- [ ] Ambiente da casa da avó (silêncio pesado)
- [ ] Corte de ambiência às 04:00 — a vila fica sem murmúrio nenhum

## 1.4 SFX — Chapéuzinho

- [ ] Passos na **grama** (2+ variações, sorteadas)
- [ ] Passos no **ladrilho** da rua principal (2+ variações)
- [ ] Passos no **pedregulho** das trilhas (2+ variações)
- [ ] Passos em **piso interno** (casas, loja, escola, cabana)
- [ ] Passos **agachada** — versão abafada e mais lenta
- [ ] Agachar (CTRL)
- [ ] Levantar (CTRL)
- [ ] Respiração da Chapéuzinho (variação de esforço/medo)
- [ ] Som de dano — perder 1 coração
- [ ] Som de dano — perder 2 corações (a partir da 5ª fatia)
- [ ] **Som de morte** (perder o 3º coração)
- [ ] Grito da Chapéuzinho ao ser atingida

## 1.5 SFX — Lampião e querosene

- [ ] **Acender** o lampião (F)
- [ ] **Apagar** o lampião (F)
- [ ] Chama em loop enquanto aceso (baixo, contínuo)
- [ ] Pegar **lata de querosene**
- [ ] **Abastecer** o lampião (troca automática de lata)
- [ ] Aviso de querosene acabando (último traço da barra)
- [ ] Lampião apagando sozinho por falta de combustível
- [ ] Balanço do lampião — ranger metálico da alça enquanto ela anda

## 1.6 SFX — Itens e interação

- [ ] **Pegar fatia** de torta
- [ ] **Pegar chave** (genérico, serve às 5 chaves)
- [ ] **Chave caindo no chão** (do corpo do Lobo)
- [ ] **Pegar moeda**
- [ ] Pegar / abrir **papel** (carta, bilhete, lista, anotação)
- [ ] **Pegar marreta**
- [ ] Pegar **escada**
- [ ] **Abrir porta**
- [ ] **Fechar porta**
- [ ] Porta trancada (maçaneta que não gira)
- [ ] Recusa de tranca — "não alcanço" / "está trancado"
- [ ] Prompt do E aparecendo (tique curto de UI)

## 1.7 SFX — Puzzles

- [ ] **Arrastar a escada** pelo chão (loop enquanto carrega)
- [ ] **Encostar a escada** na árvore
- [ ] Subir degraus da escada
- [ ] **Queda da torta** da copa da árvore no chão
- [ ] **Giro da marreta no vazio** (whoosh)
- [ ] **Marretada acertando** o Lobo (impacto pesado)
- [ ] **Marreta quebrando**
- [ ] **Gaiola destrancando**
- [ ] Porta de ferro da gaiola abrindo
- [ ] **Teclado da máquina** — tecla individual
- [ ] Máquina — código **correto** aceito
- [ ] Máquina — **erro (bipe alto)** que chama o Lobo
- [ ] Máquina — mola girando e **queda da fatia** na bandeja
- [ ] Inserir a moeda na máquina
- [ ] **Remover barricada** de mesas — 3 s de arrasto, barulho alto
- [ ] Barricada do saguão se soltando por dentro (atalho da escola)
- [ ] Sebes do labirinto sendo roçadas ao passar

## 1.8 SFX — Lobo

- [ ] **Uivo distante** (1ª fatia)
- [ ] Uivo próximo (variação de proximidade)
- [ ] **Lobo se aproximando** — passos pesados escalando por distância
- [ ] Respiração do Lobo escalando por distância
- [ ] Grunhido / rosnado (PATRULHA e SUSPEITA)
- [ ] Farejar (a partir da 4ª fatia)
- [ ] **Bote do Lobo** (o beat antes da mordida)
- [ ] Ataque / mordida
- [ ] **Lobo se afastando pela luz** (RECUO — até a 7ª fatia)
- [ ] Lobo levando a marretada (hit)
- [ ] **Lobo atordoado** — respiração arrastada, 15 s
- [ ] Lobo se levantando do atordoamento
- [ ] Lamento / uivo de dor
- [ ] Som da **transformação** do Lobo (cutscene)
- [ ] Grama sendo pisada pelo Lobo — só perto e **fora do quadro**
- [ ] Som do Lobo entrando no mapa (3ª fatia)
- [ ] Som do Lobo saindo do mapa (8ª fatia)

## 1.9 SFX — Mundo e eventos da noite

- [ ] **Postes piscando** (2ª fatia)
- [ ] Blecaute — as luzes da cidade se apagando (3ª fatia)
- [ ] **Postes reacendendo em vermelho** (6ª fatia) — zumbido diferente do original
- [ ] Zumbido contínuo dos postes acesos
- [ ] Troca das casas para o estado destruído (som fora da tela)
- [ ] **Rádio com chiado** (casa sul)
- [ ] Escola destrancando (6ª fatia)
- [ ] Casa da avó destrancando (8ª fatia)
- [ ] **Grito do NPC da janela** (ao pegar a fatia nº 8)
- [ ] Vidro da janela do NPC quebrando
- [ ] Água do poço (praça central)
- [ ] Marcação de hora do relógio a cada fatia coletada

## 1.10 SFX — Falas, legendas e UI

- [ ] **Falas dos moradores** — murmúrio (6 variações, uma por casa)
- [ ] Murmúrio de ambientação dos moradores que não falam
- [ ] Fala do NPC da janela — linha A (3ª fatia)
- [ ] Fala do NPC da janela — linha B (6ª fatia)
- [ ] **Som de caractere da legenda** (typewriter)
- [ ] Hover do mouse nos botões
- [ ] Clique dos botões
- [ ] **Press Start** da primeira tela
- [ ] Abrir pausa (ESC) — braço subindo
- [ ] Fechar pausa (ESC) — braço descendo
- [ ] Navegação de opções (volume, idioma)
- [ ] Som de saída / quit

## 1.11 SFX — Cutscene final

- [ ] Cesta caindo no chão
- [ ] Fatias sendo montadas sobre a mesa
- [ ] O corpo se levantando da cama
- [ ] Destransformação — pelo virando pele
- [ ] **Arma sendo carregada**
- [ ] **Tiro**
- [ ] Corte para preto — silêncio absoluto
- [ ] Relógio travando em 06:00
- [ ] Música dos créditos

## 1.12 Mixagem e especificação técnica

- [ ] Todos os arquivos exportados em **WAV 16-bit / 44.1 kHz**
- [ ] Camada de mixagem **Ambiente** (grupo próprio no AudioMixer)
- [ ] Camada de mixagem **Lobo** (grupo próprio, prioridade de leitura)
- [ ] Camada de mixagem **UI** (grupo próprio, imune ao `timeScale = 0`)
- [ ] Camada de mixagem **Música**
- [ ] Camada de mixagem **Player / SFX**
- [ ] Ducking da música quando o Lobo está perto
- [ ] Áudio espacializado (2D vs 3D) definido item por item
- [ ] Rolloff de distância calibrado para os passos do Lobo
- [ ] Controle de volume por camada no menu de opções
- [ ] Nenhum clipe estourando (headroom de -3 dB no master)

---

# 2 · Animações

## 2.1 Chapéuzinho — em pé

- [ ] Idle — 4 direções (baixo, cima, lado com flip)
- [ ] Idle — diagonais (cima-lado, baixo-lado)
- [ ] **Caminhada — baixo**
- [ ] **Caminhada — cima**
- [ ] **Caminhada — lado**
- [ ] **Caminhada — diagonal baixo-lado**
- [ ] **Caminhada — diagonal cima-lado**
- [ ] Animação de dano / hit flash
- [ ] Animação de morte

## 2.2 Chapéuzinho — agachada

- [ ] **Agachar** (transição em pé → agachada, com custo de tempo)
- [ ] **Levantar** (transição agachada → em pé, com custo de tempo)
- [ ] CrouchIdle — 4 direções
- [ ] CrouchWalk — 4 direções
- [ ] CrouchIdle / CrouchWalk — diagonais

## 2.3 Chapéuzinho — mãos ocupadas

- [ ] **Balanço do lampião na mão** (física — atrasa nas mudanças de direção)
- [ ] Oscilação do círculo de luz acompanhando o lampião
- [ ] Acender / apagar a chama
- [ ] Chama do lampião em loop (idle da luz)
- [ ] **Carregar a escada** — idle e caminhada (silhueta própria)
- [ ] **Encostar a escada** na árvore
- [ ] **Subir a escada**
- [ ] Descer a escada
- [ ] **Carregar a marreta** — idle e caminhada
- [ ] **Giro da marreta** (trava o movimento pela duração)
- [ ] **Marreta quebrando** no impacto
- [ ] **Troca de lampião por marreta** (o lampião sai da mão sozinho)
- [ ] Troca de marreta por lampião (a luz volta após o acerto)
- [ ] Abastecer o lampião com a lata

## 2.4 Lobo Mau

- [ ] Idle (forma normal)
- [ ] Caminhada / patrulha
- [ ] **Corrida do Lobo** (perseguição)
- [ ] **Farejar** (a partir da 4ª fatia)
- [ ] **Bote** — a janela de ataque telegrafada
- [ ] **Ataque** / mordida
- [ ] **Recuo** ao entrar no círculo do lampião
- [ ] **Atordoado** — 15 s no chão
- [ ] **Se levantando** do atordoamento
- [ ] Hit / levar a marretada
- [ ] Surgindo no mapa (3ª fatia)
- [ ] Desaparecendo do mapa (8ª fatia)
- [ ] Garras aparecendo (5ª fatia — variação visual)

## 2.5 Lobo — forma transformada / vovó

- [ ] Idle do Lobo vestido de vovó (4 frames, na cama)
- [ ] Levantar-se da cama
- [ ] Caminhada da forma transformada
- [ ] Ataque da forma transformada
- [ ] Idle do caçador em forma humana (4 frames)
- [ ] **Destransformação** — Lobo → caçador
- [ ] Caçador olhando as próprias mãos

## 2.6 Moradores e NPC

- [ ] Idle dos moradores — sentado
- [ ] Idle dos moradores — de costas
- [ ] Idle dos moradores — olhando pela janela
- [ ] Idle da mãe com criança de colo
- [ ] Fala única do morador mais próximo da porta
- [ ] Idle indiferente: nenhum morador reage ao Lobo, ao lampião ou às fatias
- [ ] Desaparecimento dos moradores na 6ª fatia (troca fora da tela)

## 2.7 Objetos e puzzles

- [ ] **Chave caindo do corpo do Lobo** no ponto do golpe
- [ ] Chave quicando no chão
- [ ] **Gaiola abrindo**
- [ ] **Queda da fatia na máquina** (mola girando + tombo na bandeja)
- [ ] Teclado da máquina — feedback de tecla
- [ ] Máquina — animação de erro (bipe + luz)
- [ ] Remoção da barricada de mesas (3 s)
- [ ] Barricada do saguão se soltando pelo lado de dentro
- [ ] Portas abrindo e fechando
- [ ] Fatia de torta — idle no chão
- [ ] Lata de querosene — idle
- [ ] Papéis abrindo em tela cheia (leitura)

## 2.8 Mundo e eventos da noite

- [ ] **Postes piscando** (2ª fatia) — com jitter por poste
- [ ] Postes se apagando (3ª fatia)
- [ ] **Postes reacendendo em vermelho** (6ª fatia)
- [ ] **Sebes ao vento** (labirinto)
- [ ] Árvores da floresta ao vento
- [ ] Grama ao vento
- [ ] Janela acesa do NPC — luz oscilando
- [ ] Janela do NPC apagando e quebrando (após a fatia nº 8)
- [ ] **Troca das casas para o estado destruído** (6ª fatia — instantânea, fora da tela)
- [ ] Sangue escorrendo / poças (cidade de sangue)
- [ ] Água do poço

## 2.9 Cutscene final

- [ ] **Cutscene final completa**, beat a beat, sem input
- [ ] Beat 1 — Chapéuzinho entra e monta a torta sobre a mesa
- [ ] Beat 2 — a "avó" se levanta; a cesta cai
- [ ] Beat 2b — corte para o quarto: restos, sangue, a touca da avó no chão
- [ ] Beat 3 — o Lobo avança, se contorce e se destransforma
- [ ] Beat 4 — o caçador olha as mãos e encontra o papel "não sou eu"
- [ ] Beat 4b — corte para preto + som de tiro
- [ ] Beat 5 — relógio marca 06:00 e trava
- [ ] Beat 5b — créditos sobem

## 2.10 UI e HUD

- [ ] Braço diegético subindo (abrir pausa)
- [ ] Braço diegético descendo (fechar pausa)
- [ ] Fade-in dos botões de pausa (`UIFadeIn`)
- [ ] Fade in/out da noite (início de partida)
- [ ] Fade de morte
- [ ] Transição entre menu principal e jogo
- [ ] Legenda entrando caractere a caractere
- [ ] Prompt do E aparecendo / sumindo
- [ ] Ponteiro do relógio avançando a cada fatia
- [ ] Coração se apagando na pulseira
- [ ] Traço de querosene sumindo da barra

## 2.11 Setup de Animator

- [ ] `Sprite.controller` — 4 estados (Idle, Walk, CrouchIdle, CrouchWalk)
- [ ] Cada estado como **Blend Tree 2D Simple Directional** em `(0,-1) (0,1) (-1,0) (1,0)`
- [ ] Parâmetros `MoveX`, `MoveY` (Float) e `IsMoving`, `IsCrouched` (Bool) com os nomes batendo com o `PlayerAnimatorBridge`
- [ ] Transições por `Any State`, `Has Exit Time` desmarcado, duração 0
- [ ] **`Can Transition To Self` desmarcado** em todas as transições
- [ ] `Wolf.controller` — Idle, Walk, Run, Attack, Sniff, Recoil, Stun
- [ ] `WolfTransformed.controller` — Idle, Walk, Attack
- [ ] Controller de mãos ocupadas (lampião / escada / marreta)
- [ ] Controller dos moradores
- [ ] Controller dos postes (aceso / piscando / apagado / vermelho)
- [ ] Todos os clipes com `Loop Time` correto (idle e walk sim, transições não)
- [ ] Sample rate de todos os clipes padronizado
- [ ] Animation Events sincronizando os SFX (passo, marretada, quebra)

---

# 3 · Texturas

> Pixel art densa acima do padrão 8-bit · paleta noturna · resolução interna escalada por inteiro (**nearest neighbor**) · **pixels per unit fixo** em todo o projeto · sem filtro nem suavização.

## 3.1 Personagens

- [ ] **Chapéuzinho — em pé**: idle 8 direções
- [ ] **Chapéuzinho — em pé**: walk sheet 8 direções
- [ ] **Chapéuzinho — agachada**: idle 8 direções
- [ ] **Chapéuzinho — agachada**: walk sheet 8 direções
- [ ] Chapéuzinho segurando o **lampião**
- [ ] Chapéuzinho carregando a **escada**
- [ ] Chapéuzinho carregando a **marreta**
- [ ] Chapéuzinho — frames de dano e morte
- [ ] **Lobo Mau** — idle sheet
- [ ] **Lobo Mau** — walk / run sheet
- [ ] **Lobo Mau** — attack sheet
- [ ] **Lobo Mau** — sniff / farejar sheet
- [ ] **Lobo Mau** — recuo sheet
- [ ] **Lobo atordoado** — frames no chão + levantar
- [ ] Lobo com garras (variação da 5ª fatia)
- [ ] **Lobo vestido de vovó** — idle sheet (4 frames)
- [ ] Lobo vestido de vovó — levantar da cama e avançar
- [ ] **Caçador vestido de vovó** (estado intermediário da destransformação)
- [ ] **Caçador — forma humana** idle sheet (4 frames)
- [ ] Caçador olhando as mãos
- [ ] **10 moradores** — sprites reaproveitáveis com troca de paleta
- [ ] Morador — mãe
- [ ] Morador — criança de colo
- [ ] Morador — filho mais velho
- [ ] Morador — homem velho quase surdo
- [ ] Morador — professora
- [ ] Morador — marido da professora
- [ ] Morador — pescador
- [ ] Morador — casal idoso (2 sprites)
- [ ] Morador — rapaz do rádio
- [ ] Paletas alternativas dos moradores (para repetir sprite entre casas)
- [ ] Avó — touca no chão e restos no quarto (cutscene)

## 3.2 Tilesets da cidade

- [ ] Tileset de **grama** (terreno padrão, o mesmo da floresta)
- [ ] Tileset de **ladrilho** — a rua principal norte-sul
- [ ] Tileset de **pedregulho** — trilhas estilo Saint Denis
- [ ] Autotile / regras de transição entre grama, ladrilho e pedregulho
- [ ] Tileset de **fachadas** das casas
- [ ] Tileset de telhados
- [ ] Tileset de cercas e muros
- [ ] Tileset de piso interno (madeira, azulejo)
- [ ] Tileset de paredes internas
- [ ] Tileset de **paredes rasgadas** (estado destruído)
- [ ] Tileset da escola (piso e parede institucional)
- [ ] Tileset da floresta (chão de mata, folhas)
- [ ] Colisores de todos os tilemaps configurados

## 3.3 Cenário — exteriores

- [ ] **Sebes do labirinto** (altas, bloqueiam linha de visão)
- [ ] Cantos e pontas das sebes
- [ ] Becos sem saída com sangue (2 no labirinto)
- [ ] **Poço da vila**
- [ ] **Árvores da floresta** (variações)
- [ ] **A árvore da fatia nº 4** (com ponto de encosto da escada)
- [ ] Arbustos e mato
- [ ] Mato dos fundos da cabana (onde está a chave)
- [ ] **Postes** — apagado
- [ ] **Postes** — aceso (amarelo)
- [ ] **Postes** — piscando
- [ ] **Postes** — **vermelho** (cidade de sangue)
- [ ] Luminária / lampião de rua
- [ ] Banco da entrada sul (spawn, fatia nº 1)
- [ ] Cruzamento central da rua (fatia nº 2)
- [ ] Placas e detalhes de rua
- [ ] Janela acesa do NPC (amarela)
- [ ] Janela do NPC quebrada e apagada

## 3.4 Cenário — casas dos moradores

- [ ] Fachada da casa **noroeste** — íntegra e destruída
- [ ] Fachada da casa **nordeste** — íntegra e destruída
- [ ] Fachada da casa **oeste** — íntegra e destruída
- [ ] Fachada da casa **sudeste** — íntegra e destruída
- [ ] Fachada da casa **sudoeste** — íntegra e destruída
- [ ] Fachada da casa **sul** — íntegra e destruída
- [ ] Fachada da **casa do NPC** (nunca entrável)
- [ ] Casa do NPC em ruínas (só após a fatia nº 8)
- [ ] **Interior íntegro** — 6 variações de sala (uma por casa)
- [ ] **Interior destruído** — 6 variações (porta arrancada, parede rasgada, móveis virados)
- [ ] Móveis: cama, mesa, cadeira, armário, estante, fogão
- [ ] Móveis virados / quebrados
- [ ] Rádio ligado (casa sul) + segunda cadeira vazia
- [ ] Portas — fechada, aberta, arrancada

## 3.5 Cenário — escola

- [ ] **Saguão de entrada**
- [ ] **Corredor oeste**
- [ ] **Corredor leste**
- [ ] **Sala de aula** (carteiras viradas, desenhos nas paredes)
- [ ] **Secretaria**
- [ ] **Refeitório** (mesas)
- [ ] **Sala dos professores**
- [ ] **Armário** trancado (fatia nº 6)
- [ ] Fachada externa da escola — trancada
- [ ] Fachada externa da escola — destrancada (6ª fatia)
- [ ] **Barricada fixa** do corredor leste
- [ ] **Barricada removível** de mesas (refeitório)
- [ ] Barricada do saguão — solta pelo lado de dentro
- [ ] **Planta da escola** afixada na parede (único mapa de interior do jogo)
- [ ] Desenhos de criança nas paredes
- [ ] Quadro-negro

## 3.6 Cenário — cabana, loja e casa da avó

- [ ] **Cabana do caçador** — exterior
- [ ] Cabana — interior com rasgos nas paredes
- [ ] Carne humana e sangue nos cômodos da cabana
- [ ] **Gaiola de ferro — fechada**
- [ ] **Gaiola de ferro — aberta**
- [ ] **Marreta — inteira**
- [ ] **Marreta — quebrada**
- [ ] **Loja de conveniência** — exterior (luzes sempre acesas)
- [ ] Loja — interior (balcão, prateleiras, vazia)
- [ ] **Máquina de snacks com teclado**
- [ ] Máquina — display do código
- [ ] Máquina — estado de erro
- [ ] **Casa da avó** — exterior selado
- [ ] Casa da avó — exterior destrancado (8/8)
- [ ] Casa da avó — sala com a mesa da torta
- [ ] Casa da avó — quarto: restos, sangue, touca no chão

## 3.7 Itens

- [ ] **Fatia de torta** (sprite do coletável)
- [ ] Torta montada sobre a mesa (8 fatias — cutscene)
- [ ] Cesta da Chapéuzinho
- [ ] **Lampião de querosene** — apagado
- [ ] **Lampião de querosene** — aceso (com chama animada)
- [ ] **Lata de querosene**
- [ ] **Escada**
- [ ] **Chave da cabana**
- [ ] **Chave da gaiola**
- [ ] **Chave da secretaria**
- [ ] **Chave do refeitório**
- [ ] **Chave do armário**
- [ ] **Moeda**
- [ ] **Folha de estoque**
- [ ] **Carta da mãe**
- [ ] **Bilhete do vizinho**
- [ ] **Bilhete da professora**
- [ ] **Bilhete do aluno**
- [ ] **Lista de chamada**
- [ ] **Última anotação**
- [ ] **Papel da cabana** ("não sou eu" repetido)
- [ ] Textura de fundo para leitura de papel em tela cheia
- [ ] Arma do caçador (cutscene final)

## 3.8 Sangue e cidade de sangue

- [ ] **Poças de sangue** (variações)
- [ ] **Respingos de sangue** em parede
- [ ] Respingos de sangue no **teto**
- [ ] Sangue no chão de terra / grama
- [ ] Trilha de sangue da Chapéuzinho (rastro da 4ª fatia)
- [ ] Marcas de garra em parede
- [ ] Overlay / tint vermelho da cidade de sangue

## 3.9 UI, HUD e menus

- [ ] Arte da tela de **menu principal**
- [ ] Sprite do lobo do menu principal
- [ ] **Botões** — normal, hover, pressionado, desabilitado
- [ ] Fundo escuro da pausa
- [ ] **Braço diegético** (`Arm.png`)
- [ ] **Relógio do braço** — 8 quadros (23:00 → 06:00)
- [ ] **Relógio do braço** — quadro de abertura 22:00 (`clock22.png`)
- [ ] **Pulseira** — 3 quadros (3 → 1 coração)
- [ ] **Latas de querosene amarradas** — 5 quadros (4 → 0 reservas)
- [ ] Barra de querosene (4 traços de 15 s)
- [ ] Ícones do menu
- [ ] Faixa escura de fundo da legenda
- [ ] Ícone / tecla do prompt do E
- [ ] Tela de créditos
- [ ] Tela preta com o relógio travado em 06:00
- [ ] Ícone da janela / do executável
- [ ] Splash / logo By The Sword

## 3.10 Setup técnico das texturas

- [ ] **Pixels Per Unit fixo** em todos os sprites do projeto
- [ ] Filter Mode **Point (no filter)** em tudo
- [ ] Compression **None** nos sprites de pixel art
- [ ] Pivots padronizados (base do sprite, para o Y-sorting)
- [ ] Sprites com material **`Sprite-Lit-Default`** para reagir à luz
- [ ] Sprite sheets fatiadas no Sprite Editor com grid consistente
- [ ] Sprite Atlas montado para reduzir draw calls
- [ ] `Graphics > Transparency Sort Mode` = Custom Axis `(0,1,0)`
- [ ] Sorting layers definidas (Chão, Cenário, Personagens, Frente, VFX, UI)
- [ ] Nenhuma textura com bleeding no atlas
- [ ] Paleta noturna aplicada consistentemente em todos os assets
- [ ] Densidade de pixel art coerente com o mock de menu (referência do GDD §04)

---

# 4 · Assets

## 4.1 Cenas

- [ ] `MainMenu.unity` — primeira tela com botões funcionais
- [ ] `Game.unity` — a cidade inteira em cena única
- [ ] Cena (ou setor) da **cutscene final**
- [ ] Créditos (cena ou painel dentro do Game)
- [ ] Build Settings com todas as cenas na ordem certa
- [ ] Nenhuma cena de teste sobrando no build

## 4.2 Prefabs — personagens

- [ ] `Player.prefab` — Rigidbody2D, Collider2D, AudioSource, scripts na raiz, `Sprite` e `LanternLight` como filhos
- [ ] `Wolf.prefab` — IA, `EnemyHabilities`, AudioSource e spawn point ligados
- [ ] Prefab do morador (base, com paleta trocável)
- [ ] Prefab do NPC da janela (só a janela acesa, nunca renderizado)
- [ ] Prefab do ator da cutscene (Lobo-vovó / caçador no mesmo `SpriteRenderer`)

## 4.3 Prefabs — itens (9 tipos · 20 unidades no mapa)

- [ ] `SliceOfPie.prefab` — a fatia de torta (× 8)
- [ ] Prefab do **lampião de querosene** (inicial, com a Chapéuzinho)
- [ ] Prefab da **lata de querosene** (× 10)
- [ ] Prefab da **escada** (× 1)
- [ ] Prefab da **chave da cabana** (× 1)
- [ ] Prefab da **marreta** (× 1)
- [ ] Prefab da **chave da gaiola** (× 1, carregada pelo Lobo)
- [ ] Prefab da **moeda** (× 1)
- [ ] Prefab da **folha de estoque** (× 1)
- [ ] Prefab das **chaves da escola** (× 3: secretaria, refeitório, armário)
- [ ] Prefab de papel legível (base para os 7 textos)

## 4.4 Prefabs — mundo

- [ ] `poste1.prefab` — poste com `StreetLamp` e `Light2D` no filho
- [ ] Prefab de porta de casa (com `Teleporter`)
- [ ] Prefab de porta trancada (com `ItemRequirement`)
- [ ] Prefab da barricada fixa
- [ ] Prefab da barricada removível
- [ ] Prefab da gaiola de ferro
- [ ] Prefab da máquina com teclado
- [ ] Prefab da sebe do labirinto
- [ ] Prefab do poço
- [ ] Prefab da árvore com ponto de encosto de escada
- [ ] Prefab da casa (estado íntegro + destruído no mesmo objeto)
- [ ] Prefab do rádio
- [ ] Prefabs de decoração (armário, luminária, árvores, casas 1–3)

## 4.5 Prefabs — UI

- [ ] Canvas `HUD` — Scale With Screen Size, `160×144`, Match **Height**
- [ ] Prefab do `Prompt` (Image + `InteractPrompt` + TextMeshProUGUI filho, bottom-center 150×16, Y 12)
- [ ] Prefab do painel de **Pausa** (`Background` + `Buttons` com `CanvasGroup` + `UIFadeIn`)
- [ ] Prefab do **ArmHUD** (`Arm`, `Clock`, `Bracelet`, `Battery` no mesmo rect)
- [ ] Prefab da **legenda** (caixa de subtitle)
- [ ] Prefab do fade preto (`CanvasGroup` full screen)
- [ ] Prefab da tela de leitura de papel
- [ ] **EventSystem** presente em toda cena com botão
- [ ] Ordem de irmãos correta: `Prompt` acima de `Pause`, `Fade` e `MainMenu`

## 4.6 Fontes e texto

- [ ] `PixeloidSans` — TTF + SDF asset
- [ ] `PixeloidSansBold` — TTF + SDF asset
- [ ] `PixeloidMono` — TTF + SDF asset
- [ ] Atlas SDF gerado com o `PixelFontSdfTool` (sem suavização)
- [ ] Licença das fontes incluída no repositório
- [ ] Material preset de texto legível sobre cenário escuro

## 4.7 Textos do jogo (7 textos + falas)

- [ ] **Carta da mãe** — objetivo + aviso das habilidades do Lobo
- [ ] **Bilhete do vizinho** — o homem da cabana passa pela vila de madrugada
- [ ] **Bilhete da professora** — por que a escola foi barricada por dentro
- [ ] **Bilhete do aluno** — "o homem da cabana veio de novo perguntar quantos alunos faltaram"
- [ ] **Lista de chamada** — nomes riscados + data da última aula
- [ ] **Última anotação** — "trancamos por dentro… NÃO SOU EU."
- [ ] **Papel da cabana** — "não sou eu" repetido até o fim da folha
- [ ] Os três textos da mesma mão com caligrafia coerente (professora → anotação → cabana)
- [ ] Fala da casa noroeste — "Fecha a porta. Ele já passou aqui uma vez hoje."
- [ ] Fala da casa nordeste — "Não ouço mais nada desde as dez. Isso é bom ou é ruim?"
- [ ] Fala da casa oeste — "A escola está fechada. Barricamos por dentro. Não foi por causa de bicho."
- [ ] Fala da casa sudeste — "Leva. Eu não vou mais subir em nada."
- [ ] Fala da casa sudoeste — "A gente já contou as horas. Falta pouco."
- [ ] Fala da casa sul — "Meu irmão saiu pra ver o que era o barulho. Já faz tempo."
- [ ] **Linha A do NPC** (3ª fatia) — indica a fatia nº 8
- [ ] **Linha B do NPC** (6ª fatia) — "Não bate mais nas outras portas."
- [ ] Prompts contextuais do E (pegar, entrar, trancado, não alcanço, derrubar)
- [ ] Textos de menu, opções e créditos
- [ ] Tradução completa **PT-BR**
- [ ] Tradução completa **Inglês**
- [ ] Tabela de localização com todas as chaves

## 4.8 Iluminação 2D

- [ ] **Global Light 2D** calibrado por hora da noite (fora do círculo o mapa é quase preto)
- [ ] `Light2D` do lampião — **Spot**, Inner/Outer Angle **360**, Outer Radius ~5
- [ ] `Light2D` de cada poste (amarelo)
- [ ] Preset vermelho dos postes (6ª fatia)
- [ ] `Light2D` da janela acesa do NPC
- [ ] `Light2D` da loja de conveniência (`alwaysOn`)
- [ ] `Light2D` interno das casas íntegras
- [ ] Luz da chama com flicker sutil
- [ ] Blend Styles do URP 2D configurados e nomeados
- [ ] Bloqueio de linha de visão das sebes do labirinto
- [ ] Curva de escurecimento por fatia (22:00 → 06:00) validada em playtest

## 4.9 Materiais e render

- [ ] `Sprite-Lit-Default` aplicado a tudo que reage à luz
- [ ] Material unlit para UI e HUD
- [ ] `UniversalRenderPipelineGlobalSettings` conferido
- [ ] **Pixel Perfect Camera** configurada (Assets PPU, Reference Resolution, Upscale RT)
- [ ] Resolução interna escalada por inteiro, sem filtro
- [ ] Post-processing mínimo (vinheta / grain) sem quebrar o pixel

## 4.10 Distribuição dos itens no mapa (35 objetos)

- [ ] Fatia **nº 1** — banco da entrada sul (spawn, sem puzzle)
- [ ] Fatia **nº 2** — cruzamento central da rua (sem puzzle)
- [ ] Fatia **nº 3** — centro do labirinto, ao lado do poço
- [ ] Fatia **nº 4** — topo da árvore, floresta noroeste
- [ ] Fatia **nº 5** — gaiola dentro da cabana
- [ ] Fatia **nº 6** — armário da escola
- [ ] Fatia **nº 7** — máquina da loja
- [ ] Fatia **nº 8** — floresta sudoeste
- [ ] Lata de querosene 1 — casa noroeste
- [ ] Lata de querosene 2 — casa oeste
- [ ] Lata de querosene 3 — casa sudoeste
- [ ] Lata de querosene 4 — casa sul
- [ ] Lata de querosene 5 — cabana do caçador
- [ ] Lata de querosene 6 — loja de conveniência
- [ ] Lata de querosene 7 — escola
- [ ] Lata de querosene 8 — labirinto
- [ ] Lata de querosene 9 — ao ar livre
- [ ] Lata de querosene 10 — ao ar livre
- [ ] Escada — dentro da casa sudeste
- [ ] Chave da cabana — no mato atrás da casa do caçador
- [ ] Marreta — encostada na parede da cabana
- [ ] Chave da gaiola — no corpo do Lobo
- [ ] Moeda — balcão da loja
- [ ] Folha de estoque — atrás do balcão da loja
- [ ] Chave da secretaria — sala de aula
- [ ] Chave do refeitório — secretaria
- [ ] Chave do armário — sala dos professores
- [ ] Carta da mãe — ponto de spawn
- [ ] Bilhete do vizinho — casa nordeste
- [ ] Bilhete da professora — casa oeste
- [ ] Bilhete do aluno — sala de aula
- [ ] Lista de chamada — secretaria
- [ ] Última anotação — sala dos professores
- [ ] Papel da cabana — dentro da cabana
- [ ] **Total conferido: 8 fatias + 20 unidades de item + 7 textos**

## 4.11 Level design montado

- [ ] Rua principal retilínea norte-sul em ladrilho
- [ ] Trilhas de pedregulho ligando cada fachada
- [ ] Grama fora da rua e das trilhas (sem calçada, sem fronteira vila/mata)
- [ ] 6 casas entráveis posicionadas (NO, NE, O, SE, SO, S)
- [ ] Casa do NPC na trilha leste
- [ ] Praça do poço com o labirinto de sebes
- [ ] Escola posicionada e trancada
- [ ] Cabana do caçador — o ponto mais distante do mapa
- [ ] Loja de conveniência
- [ ] Casa da avó no fim da rua principal
- [ ] Floresta noroeste (árvore da fatia nº 4)
- [ ] Floresta sudoeste (fatia nº 8)
- [ ] Colisores de todo o cenário
- [ ] Rotas de patrulha do Lobo pela rua principal
- [ ] Spawn point do Lobo (3ª fatia)
- [ ] Pontos de espera do Lobo nas saídas dos interiores
- [ ] Nenhum ponto do mapa em que a jogadora fique presa

## 4.12 Configuração de projeto

- [ ] Estrutura `Assets/_Project/{Art, Audio, Prefabs, Scenes, Scripts, UI}` respeitada
- [ ] Layers e Tags definidas (Player, Enemy, Interactable, Shelter)
- [ ] Layer própria para interagíveis (o buffer do `OverlapCircleNonAlloc` é 8)
- [ ] Input map dos 8 botões: `W A S D · E · F · CTRL · ESC`
- [ ] Physics 2D — matriz de colisão configurada
- [ ] `.gitignore` do Unity conferido
- [ ] Commits por sistema (padrão do GDD)
- [ ] Build Windows x64 gerada e testada fora do editor
- [ ] Ícone e nome da janela do executável
- [ ] Sem sistema de save (fora de escopo — confirmar que nada persiste)
- [ ] GDD versionado junto do repositório

---

# 5 · Scripts

> `Assets/_Project/Scripts/` · `Noise` é classe estática e `IInteractable` é interface — nenhum dos dois vai em GameObject.

## 5.1 Player

- [ ] `Player/PlayerController.cs` — movimento WASD, velocidade única, sem stamina
- [ ] `PlayerController` — `SetCrouched` / `IsCrouched` (CTRL)
- [ ] `PlayerController` — multiplicador de velocidade agachada (0.5)
- [ ] `PlayerController` — custo de tempo da animação ao levantar
- [ ] `PlayerController` — `MovementLocked` para cutscene e menu
- [ ] `Player/PlayerHealth.cs` — 3 corações, dano variável (1 → 2 na 5ª fatia), sem cura
- [ ] `PlayerHealth` — invulnerabilidade curta, knockback e `OnHeartsChanged`
- [ ] `Player/PlayerHitFlash.cs` — flash do sprite ao levar dano
- [ ] `Player/PlayerInteractor.cs` — raio do E e `OnFocusChanged(alvo, texto)`
- [ ] `PlayerInteractor` — regra do GDD: **agachada não interage com itens**
- [ ] `Player/PlayerInventory.cs` — roteia por `itemId`, conta fatias 0–8, `OnAllSlicesCollected`
- [ ] `PlayerInventory` — `Consume` recusa o `sliceItemId` de propósito
- [ ] `Player/PlayerFootsteps.cs` — `stepClips` sorteados, `stepInterval`, `crouchVolume`
- [ ] `PlayerFootsteps` — som por **superfície** (grama, ladrilho, pedregulho, piso interno)
- [ ] `Player/PlayerAnimatorBridge.cs` — escreve `MoveX`, `MoveY`, `IsMoving`, `IsCrouched`
- [ ] `Player/Lantern.cs` — F acende e apaga; apagado não queima
- [ ] `Lantern` — lata = 60 s (4 traços × 15 s), troca automática, `OnSpareBatteriesChanged`
- [ ] `Lantern` — `Illuminates(ponto)` para a IA decidir o RECUO
- [ ] `Lantern` — renomear bateria → **querosene** (GDD v1.6: nada de elétrico no jogo)
- [ ] **Novo:** `Player/LanternSway.cs` — física do balanço do lampião e do círculo de luz
- [ ] **Novo:** `Player/HandSlot.cs` — slot único de mãos: lampião / escada / marreta nunca coexistem
- [ ] **Novo:** `Player/Hammer.cs` — F gira, trava o movimento, quebra ao acertar, 1 acerto por partida
- [ ] **Novo:** `Player/Ladder.cs` — carregar, encostar na árvore e subir
- [ ] **Novo:** `Player/BloodTrail.cs` — o rastro que o Lobo fareja a partir da 4ª fatia

## 5.2 Inimigo — o Lobo

- [ ] `Enemy/EnemyMov.cs` — estado **PATRULHA** por pontos do mapa
- [ ] `EnemyMov` — estado **SUSPEITA** alimentado por `Noise.OnNoise`
- [ ] `EnemyMov` — estado **PERSEGUIÇÃO** por linha de visão
- [ ] `EnemyMov` — estado **RECUO** dentro do círculo do lampião aceso
- [ ] `EnemyMov` — agachada, ele não a percebe (nem por proximidade nem por visão)
- [ ] `EnemyMov` — pathfinding que não trave em sebe, parede ou mobília
- [ ] `Enemy/EnemyAtk.cs` — alcance, cooldown, recuo pós-ataque e `Damage` variável
- [ ] **Novo:** estado **BOTE** — janela de ataque telegrafada antes da mordida
- [ ] **Novo:** estado **ATORDOADO** — 15 s, uma vez por partida, disparado pela marretada
- [ ] **Novo:** `Enemy/EnemyScent.cs` — rastreamento por trilha de sangue (4ª fatia)
- [ ] **Novo:** `Enemy/EnemyShelterRule.cs` — não invade interiores íntegros, **espera na saída**
- [ ] **Novo:** o Lobo entra nos interiores **arruinados** depois da 6ª fatia
- [ ] **Novo:** suspeita disparada por porta aberta, barricada removida e erro no teclado
- [ ] `Enemy/EnemyHabilities.cs` — tabela `WolfStage` 0–8, idempotente, `ApplyStage(0)` reinicia
- [ ] `EnemyHabilities` — reescreve presença, `SenseRadius`, `SightRadius`, `ChaseSpeed`, `Damage`, `FearsLight`
- [ ] `EnemyHabilities` — `cue` (som da hora) e `onReached` (`UnityEvent` de ambientação) por estágio
- [ ] `EnemyHabilities` — estágio 1 (23:00) uivo distante
- [ ] `EnemyHabilities` — estágio 2 (00:00) postes piscam
- [ ] `EnemyHabilities` — estágio 3 (01:00) blecaute + Lobo entra + linha A do NPC
- [ ] `EnemyHabilities` — estágio 4 (02:00) faro do sangue
- [ ] `EnemyHabilities` — estágio 5 (03:00) garras, dano 2
- [ ] `EnemyHabilities` — estágio 6 (04:00) cidade de sangue
- [ ] `EnemyHabilities` — estágio 7 (05:00) **a luz vira isca** (emite ruído na posição da luz)
- [ ] `EnemyHabilities` — estágio 8 (06:00) o Lobo some do mapa sem ser destruído
- [ ] `Enemy/EnemyAudio.cs` — passos e respiração escalando por distância
- [ ] `EnemyAudio` — `grassClips` só com o Lobo perto e **fora do quadro**
- [ ] `Enemy/EnemyAnimatorBridge.cs` — estados da IA → parâmetros do Animator
- [ ] **Novo:** `Enemy/WolfKeyDrop.cs` — a chave da gaiola cai no ponto exato do golpe

## 5.3 Interação

- [ ] `Interaction/IInteractable.cs` — `Prompt`, `CanInteract`, `Interact`
- [ ] `Interaction/Collectible.cs` — `itemId`, `displayName`, `amount`, `promptOverride`
- [ ] `Collectible` — seção **Tranca** (`ItemRequirement`) e `SetLocked(bool)`
- [ ] `Interaction/ItemRequirement.cs` — `itemId`, `amount`, `consume`, `lockedPrompt`, `lockedMessage`, `lockedClip`, `onLockedAttempt`
- [ ] `ItemRequirement.OnAnyDenied` — evento estático que o `InteractPrompt` já escuta
- [ ] `ItemRequirement` — trancado **continua** respondendo ao E (regra que não deve ser "consertada")
- [ ] `Interaction/KnockDown.cs` — derrubar a fatia da copa antes de coletar
- [ ] `Interaction/Teleporter.cs` — `destination`, `snapCamera`, `stopOnArrival`, `singleUse`, `onTeleported`
- [ ] `Teleporter.OnAnyTeleport` — aviso por código
- [ ] **Novo:** `Interaction/Readable.cs` — os 7 textos, leitura em tela cheia com E
- [ ] **Novo:** `Interaction/Door.cs` — porta com estado, som e suspeita do Lobo ao abrir
- [ ] **Novo:** `Interaction/Barricade.cs` — remoção com E em 3 s, barulho alto, emite `Noise`
- [ ] **Novo:** `Interaction/Cage.cs` — a gaiola de ferro da fatia nº 5
- [ ] **Novo:** `Interaction/VendingMachine.cs` — moeda + código; o bipe do erro emite ruído
- [ ] **Novo:** `Interaction/Keypad.cs` — entrada do código pelo teclado da máquina

## 5.4 Sistemas

- [ ] `Systems/Clock.cs` — começa em 22:00 e avança 1 h por fatia até 06:00
- [ ] `Clock` — `OnHourChanged(hora, texto)`, `TimeText`, `HoursElapsed` (0–8)
- [ ] `Systems/NightRunner.cs` — morte, `deathDelay`, fade e recarga da cena
- [ ] `NightRunner` — congela o Lobo no instante da morte
- [ ] `NightRunner` — `onNightSurvived` na oitava fatia (porta da avó abre, Lobo sai)
- [ ] `Systems/EndingSequence.cs` — cutscene final beat a beat, input travado
- [ ] `EndingSequence` — troca Lobo-vovó → caçador no mesmo `SpriteRenderer`
- [ ] `EndingSequence` — som da arma, tiro, corte para preto e créditos
- [ ] `Systems/PauseMenu.cs` — ESC, `pausePanel`, `Resume/OpenOptions/GoToMainMenu/QuitGame`
- [ ] **Novo:** `Systems/BloodCityEvent.cs` — o evento da 6ª fatia inteiro
- [ ] **Novo:** `Systems/HouseState.cs` — íntegro ↔ destruído, troca instantânea fora da tela
- [ ] `HouseState` — os objetos (latas, escada) permanecem no meio dos escombros
- [ ] **Novo:** `Systems/SchoolLock.cs` — a escola só destranca com a cidade de sangue
- [ ] **Novo:** `Systems/GrannyDoor.cs` — a casa da avó destranca em 8/8 (transição irreversível)
- [ ] **Novo:** `Systems/HammerGuard.cs` — bloqueia a marreta enquanto o Lobo não está no mapa
- [ ] **Novo:** `Systems/NightLighting.cs` — Global Light 2D por hora da noite
- [ ] **Novo:** validação de que **nenhuma ordem de coleta gera deadlock** (GDD §09)

## 5.5 Mundo

- [ ] `World/StreetLamp.cs` — aceso (0–1), piscada (2), apagado (3+)
- [ ] `StreetLamp` — `flickerAtSlice`, `blackoutAtSlice`, `flickerJitter`, `alwaysOn`
- [ ] `StreetLamp` — `StartFlicker()` para testar sem coletar duas fatias
- [ ] `StreetLamp` — **reacender em vermelho** na 6ª fatia (GDD v1.6)
- [ ] **Novo:** `World/Villager.cs` — fala única do morador mais próximo da porta
- [ ] `Villager` — os demais moradores são ambientação e não reagem a nada
- [ ] **Novo:** `World/WindowNPC.cs` — linha A (3ª fatia), linha B (6ª fatia), grito e queda da casa
- [ ] `WindowNPC` — a casa só cai **depois** da fatia nº 8, antes ou depois da cidade de sangue
- [ ] **Novo:** `World/Radio.cs` — o rádio com chiado da casa sul
- [ ] **Novo:** `World/Hedge.cs` — bloqueio de linha de visão do labirinto
- [ ] **Novo:** `World/SurfaceTag.cs` — marca a superfície para o som de passo certo

## 5.6 Áudio

- [ ] `Audio/Noise.cs` — `Noise.Emit(pos, raio, source)` e `Noise.OnNoise` (classe estática)
- [ ] `Audio/MusicDirector.cs` — faixas de fundo e de tensão, nunca simultâneas
- [ ] `MusicDirector` — crossfade por duas `AudioSource`, silêncio real entre faixas
- [ ] `MusicDirector` — `backgroundTracks`, `tensionTracks` e volumes separados
- [ ] **Novo:** `Audio/AmbienceZone.cs` — troca de ambiência por área (casa, floresta, escola, loja)
- [ ] **Novo:** `Audio/AudioMixerBridge.cs` — volume por camada no menu de opções

## 5.7 UI

- [ ] `UI/ArmHUD.cs` — braço diegético que sobe junto com a pausa
- [ ] `ArmHUD` — relógio (8 sprites, 23:00→06:00) + `nightStartSprite` (22:00)
- [ ] `ArmHUD` — pulseira (3 sprites) por `PlayerHealth.OnHeartsChanged`
- [ ] `ArmHUD` — latas de reserva (5 sprites) por `Lantern.OnSpareBatteriesChanged`
- [ ] `ArmHUD` — redesenho por evento, nunca por frame
- [ ] `UI/InteractPrompt.cs` — prompt do E; esconde o **texto**, não o objeto
- [ ] `InteractPrompt` — `background` opcional e `Flash("texto")`
- [ ] `InteractPrompt` — escuta `ItemRequirement.OnAnyDenied` sozinho
- [ ] `UI/UIFadeIn.cs` — fade em tempo real (imune ao `timeScale = 0`), bloqueia clique durante o fade
- [ ] `UI/UISounds.cs` — hover e clique automáticos nos `Button` filhos; `PlayStart()` manual
- [ ] **Novo:** `UI/Subtitles.cs` — legendas com som por caractere (falas, NPC, cutscene)
- [ ] **Novo:** `UI/Localization.cs` — troca PT-BR / Inglês
- [ ] **Novo:** `UI/MainMenu.cs` — botões da primeira tela (hoje é só arte)
- [ ] **Novo:** `UI/OptionsMenu.cs` — volume por camada e idioma
- [ ] **Novo:** `UI/KerosineBar.cs` — a barra de querosene (4 traços × 15 s)
- [ ] **Novo:** `UI/CreditsRoll.cs` — créditos após o tiro

## 5.8 Câmera e Editor

- [ ] `Camera/CameraFollow.cs` — amortecimento e snap para o `Teleporter`
- [ ] `Editor/PixelFontSdfTool.cs` — geração dos atlas SDF sem suavização
- [ ] **Novo:** `Editor/SliceMapValidator.cs` — confere as 8 fatias, 10 latas e 7 textos na cena
- [ ] **Novo:** `Editor/StageTableInspector.cs` — visualiza a tabela de 8 estágios do Lobo

## 5.9 Balanceamento e documentação

- [ ] `moveSpeed` 3.5 · `crouchSpeedMultiplier` 0.5 · `stepNoiseRadius` 6
- [ ] `invulnerabilityDuration` 1 s · `Outer Radius` da luz ~5
- [ ] 10 latas × 60 s = até 10 min de chama em uma partida de 15–30 min
- [ ] Plano B do querosene: cortar para **30 s por lata** antes de mexer na quantidade
- [ ] Enquanto ela está dentro de uma casa, o Lobo continua se movendo
- [ ] `Scripts/README.md` atualizado com todo script novo
- [ ] Todo script novo com `<summary>` explicando o porquê, no padrão dos existentes
- [ ] Nenhum `Debug.Log` sobrando na build final
- [ ] Nenhum `-=` faltando em `OnDisable` (assinaturas de evento estático)
