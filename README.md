# 8 Slices of Pie

**Survival horror / stealth de loop · top-down 2D · Unity 2022.3.62f3 (LTS) · pipeline 2D URP · PC (Windows)**
Estúdio **By The Sword** · GDD **v1.5** (agosto de 2026) · 15–30 min de partida · 17 dias de produção

> Chapéuzinho Vermelho precisa atravessar uma cidadezinha no meio da noite para recuperar **8 fatias de torta** antes de chegar à casa da avó. Cada fatia adianta o relógio em uma hora, e às 04:00 os moradores simplesmente não estão mais lá. O único escudo é um lampião elétrico com bateria contada.

---

## Como ler este checklist

Este documento é o **espelho completo do GDD v1.5** em forma de tarefas. Nada foi resumido: todo item das listas de assets (seção 11), todo sistema, todo som, toda animação, todo texto e todo puzzle do documento tem uma linha aqui.

| Marca | Significado |
|---|---|
| `- [x]` | **Pronto** — existe no repositório e está funcionando |
| `- [ ]` 🟡 | **Parcial** — já tem base no projeto, falta terminar (o que falta está escrito na linha) |
| `- [ ]` | **Pendente** — ainda não existe |

**Regra de atualização:** só marque `[x]` depois que o item estiver **na cena/branch principal e testado em play mode**. Asset importado mas não usado continua 🟡.

**Referências rápidas:** GDD §01 visão geral · §02 narrativa · §03 personagens · §04 gameplay/controles · §05 loop e progressão do Lobo · §06 mapa · §07 casas e cidade de sangue · §08 itens e textos · §09 puzzles · §10 escola · §11 produção e assets.

---

## Progresso por área

| Área | Itens | ✅ Pronto | 🟡 Parcial | ⬜ Pendente | Progresso |
|---|---:|---:|---:|---:|---:|
| **1 · Funcionalidades** | 151 | 63 | 3 | 85 | 43% |
| **2 · Mapa e level design (GDD §06)** | 22 | 0 | 4 | 18 | 9% |
| **3 · Sprites (GDD §11 — lista de assets)** | 52 | 18 | 5 | 29 | 39% |
| **4 · Animações (GDD §11 — lista de assets)** | 24 | 4 | 1 | 19 | 19% |
| **5 · Sons — SFX (GDD §11 — lista de assets)** | 45 | 13 | 2 | 30 | 31% |
| **6 · Música e trilha (GDD §11)** | 11 | 0 | 2 | 9 | 9% |
| **7 · Textos e falas (GDD §08)** | 21 | 1 | 0 | 20 | 5% |
| **8 · Técnico, build e otimização (GDD §11)** | 25 | 6 | 0 | 19 | 24% |
| **TOTAL** | **351** | **105** | **17** | **229** | **32%** |

> Progresso conta parcial como meio item. Total de **351 itens** rastreados neste checklist.

---

# 1 · Funcionalidades

## 1.1 Player — Chapéuzinho (GDD §04)

- [x] Movimento top-down com **W A S D**, velocidade única, sem stamina e sem corrida (`PlayerController`)
- [x] Facing travado nas direções cardeais para animação e interação
- [x] **CTRL** alterna agachar / levantar (`PlayerController.SetCrouched`)
- [x] Agachada: multiplicador de velocidade (0.5) e passos mais silenciosos
- [x] Agachada o Lobo **não a percebe** — nem por proximidade nem por visão (`EnemyMov`)
- [ ] Agachada ela **não interage com itens** (regra do GDD §04 ainda não aplicada no `PlayerInteractor`)
- [ ] Custo de tempo da animação ao sair do agachado (hoje a troca é instantânea)
- [x] **3 corações**, sem ataque e sem cura no mapa (`PlayerHealth`)
- [x] Invulnerabilidade curta + knockback + flash ao levar dano (`PlayerHealth`, `PlayerHitFlash`)
- [x] Dano variável informado por quem bate (1 coração; 2 a partir da 5ª fatia)
- [x] Trava de controle sem desligar o componente (`MovementLocked`) para cutscene/menu
- [x] Câmera com amortecimento seguindo a jogadora (`CameraFollow`)
- [ ] Slot de mãos exclusivo: **lampião / escada / marreta** nunca coexistem (regra central do GDD §04 e §05)

## 1.2 Lampião e bateria (GDD §04)

- [x] **F** alterna a luz; desligado não gasta carga (`Lantern`)
- [x] Círculo de luz por `Light2D` (Spot, Inner/Outer Angle 360)
- [x] Bateria = 4 barras × 15 s = **60 s de luz contínua**; drena só com o lampião aceso
- [x] Troca de bateria automática quando a carga acaba
- [x] `Illuminates(ponto)` exposto para a IA do Lobo decidir o RECUO
- [x] Coleta de bateria roteada pelo `PlayerInventory` direto para o `Lantern`
- [ ] **10 baterias** distribuídas no mapa conforme GDD §08 (hoje o `Lantern` nasce com 0 reservas e o HUD desenha 4 slots)
- [ ] Fora do círculo o mapa fica **quase preto** (Global Light 2D calibrado por hora da noite)
- [ ] Regra da 7ª fatia: lampião aceso **emite ruído** e atrai o Lobo (ver 1.4)
- [ ] Escada e marreta ocupam o lugar do lampião: com elas na mão, **F não acende nada**

## 1.3 Lobo Mau — IA (GDD §05)

- [x] Estado **PATRULHA** por pontos do mapa (`EnemyMov`)
- [x] Estado **SUSPEITA** alimentado pelo canal global de ruído (`Noise.Emit` / `Noise.OnNoise`)
- [x] Estado **PERSEGUIÇÃO** por linha de visão
- [x] Estado **RECUO** ao entrar no círculo do lampião aceso (não morde dentro da luz)
- [ ] 🟡 Estado **BOTE** como janela de ataque telegrafada (hoje existe a mordida do `EnemyAtk`, sem o beat de bote)
- [ ] Estado **ATORDOADO** — 15 s, uma vez por partida, disparado pela marretada
- [x] Mordida com alcance, cooldown e recuo pós-ataque (`EnemyAtk`)
- [x] Nunca é derrotável — não existe combate em lugar nenhum do código
- [x] Áudio como telegrafia: passos/respiração escalando por distância (`EnemyAudio`)
- [x] Som de grama só com o Lobo perto e **fora do quadro**
- [x] Spawn point próprio: entra na 3ª fatia e some na 8ª sem ser destruído
- [ ] **Rastreamento por trilha de sangue** da 4ª fatia (hoje representado só por `senseRadius` aberto)
- [ ] Ruído emitido pela luz acesa depois da 7ª fatia (a luz vira isca)
- [ ] O Lobo **não invade interiores íntegros** (casas antes da 6ª, escola, cabana, loja) — mas **espera na saída**
- [ ] O Lobo **entra nas casas arruinadas** depois da 6ª fatia (paredes rasgadas)
- [ ] Suspeita disparada por **porta aberta**, **barricada removida** e **erro no teclado da máquina**
- [ ] Pathfinding/navegação que não trave em sebe, parede e mobília

## 1.4 Progressão do Lobo — as 8 fatias (GDD §05)

- [x] Tabela de estágios `WolfStage` 0–8 no Inspector, idempotente (`EnemyHabilities`)
- [x] Cada estágio reescreve presença, `senseRadius`, `sightRadius`, `chaseSpeed`, dano e relação com a luz
- [x] `cue` (som da hora) e `onReached` (ambientação por `UnityEvent`) por estágio
- [ ] **1 fatia · 23:00** — uivo distante audível
- [ ] **2 fatias · 00:00** — postes da cidade piscam brevemente *(código pronto no `StreetLamp`, falta ligar na cena)*
- [ ] **3 fatias · 01:00** — luzes se apagam · Lobo entra no mapa · NPC da janela fala a linha A
- [ ] **4 fatias · 02:00** — Lobo sente o cheiro do sangue (rastreamento por trilha)
- [ ] **5 fatias · 03:00** — Lobo cria garras: dano sobe para 2 corações *(campo existe, falta preencher o estágio)*
- [ ] **6 fatias · 04:00** — **A CIDADE DE SANGUE**: moradores somem, casas destruídas, escola destranca
- [ ] **7 fatias · 05:00** — Lobo deixa de temer a luz e passa a ser atraído por ela
- [ ] **8 fatias · 06:00** — Lobo desaparece do mapa e a casa da avó destranca

## 1.5 Relógio e loop da noite (GDD §04, §05)

- [x] Relógio começa em **22:00** e avança 1 h por fatia, até 06:00 (`Clock`)
- [x] Hora derivada da contagem do inventário (nunca dessincroniza)
- [x] `OnHourChanged(hora, texto)` como gatilho único para quem reage a horário
- [x] Contagem de fatias 0–8 e evento de vitória (`PlayerInventory.OnAllSlicesCollected`)
- [x] Morte devolve a jogadora ao início da noite recarregando a cena (`NightRunner`)
- [x] Fade in/out da noite e o fade de morte emendados
- [x] Lobo congelado no instante da morte, para não rondar o corpo durante o fade
- [ ] Verificação de que **nenhuma ordem de coleta gera deadlock** (GDD §09)
- [ ] Guarda da fatia nº 5: coleta da **marreta bloqueada enquanto o Lobo não estiver no mapa**
- [ ] Guarda da fatia nº 6: escola só abre com a cidade de sangue (7ª ou 8ª coletada)

## 1.6 Interação, itens e inventário (GDD §04, §08)

- [x] **E** contextual com prompt na tela e busca por raio (`PlayerInteractor`)
- [x] Interface `IInteractable` para qualquer objeto do mapa
- [x] `Collectible` genérico com `itemId`, nome de exibição, quantidade e som de coleta
- [x] `PlayerInventory` roteando por id: bateria → lampião, fatia → contagem, resto → guardado
- [x] `ItemRequirement` — a tranca reutilizável ("só funciona com o item X na mão"), com prompt, recado, som e `UnityEvent` de recusa
- [x] Objeto trancado **continua respondendo ao E** (regra de design, não é bug)
- [x] `Teleporter` para passagens internas (portas, escada, buraco), com corte de câmera e uso único opcional
- [x] `KnockDown` — item alto que cai antes de ser catado (usado hoje na fatia da árvore)
- [x] Emissão de ruído no uso de itens (`Noise.Emit`) alimentando a IA
- [ ] Itens de mão que **substituem o lampião** (escada, marreta) — sistema de slot único
- [ ] Item **carregável** que muda a movimentação (escada, GDD §09 fatia nº 4)
- [ ] Objeto **legível** (papéis e bilhetes abrindo painel de leitura em tela)

## 1.7 Os 8 puzzles das fatias (GDD §09)

- [ ] **Fatia nº 1 · banco da entrada sul (spawn)** — sem puzzle, à vista
- [ ] **Fatia nº 2 · cruzamento central da rua** — sem puzzle, à vista
- [ ] **Fatia nº 3 · praça do poço** — labirinto de sebes às cegas, com dois becos sem saída terminando em sangue
- [ ] 🟡 **Fatia nº 4 · topo da árvore (floresta NO)** — trazer a **escada** da casa sudeste e encostá-la *(hoje resolvido com um `galho` + `KnockDown`; ver §9 Divergências)*
- [ ] **Fatia nº 5 · gaiola da cabana** — chave da cabana no mato → marreta → marretada no Lobo → chave da gaiola cai no ponto do golpe → gaiola abre
- [ ] **Fatia nº 6 · armário da escola** — cadeia de três chaves e três barricadas
- [ ] 🟡 **Fatia nº 7 · máquina da loja** — moeda **+ código** da folha de estoque *(moeda e máquina já entraram; falta o teclado, o código e o bipe de erro)*
- [ ] **Fatia nº 8 · floresta sudoeste** — seguir a dica do NPC da janela acesa por legenda
- [ ] Prefab da fatia com feedback de coleta, som e efeito de progresso (base em `SliceOfPie.prefab`)

## 1.8 Casas, moradores e a cidade de sangue (GDD §07)

- [ ] **6 casas entráveis**, uma sala cada, abrindo com E
- [ ] **10 moradores** distribuídos (1 a 3 por casa), sem seguir e sem reagir a nada
- [ ] Regra "**um por casa fala**": só o morador mais próximo da porta, uma única vez
- [ ] Fala única de cada casa disparada na entrada (6 falas — ver §7 Textos)
- [ ] Objetos dentro das casas nos lugares certos: 4 baterias, escada, bilhete do vizinho, bilhete da professora
- [ ] Enquanto íntegra, a casa é **abrigo**: o Lobo não entra
- [ ] O Lobo **continua se movendo** enquanto ela está dentro (a saída é sempre uma aposta cega)
- [ ] **Evento da 6ª fatia**: os 6 moradores somem sem corpo e sem explicação
- [ ] Troca de estado da casa para **arruinada** (porta arrancada, parede rasgada, móveis virados, sangue), sempre **fora da tela**
- [ ] Casa arruinada deixa de proteger: o Lobo entra e sai por onde quiser
- [ ] Os **objetos permanecem** nos escombros (quem deixou recurso para trás pode voltar)
- [ ] **Casa do NPC** (7ª casa, trilha leste): nunca entrável, nunca renderizada por dentro, só a janela acesa
- [ ] Exceção da casa do NPC: ela **só cai depois** que a jogadora pega a fatia nº 8

## 1.9 A escola (GDD §10)

- [ ] Escola **trancada e barricada a noite inteira**, destrancando só na cidade de sangue
- [ ] **1 · Saguão** com duas saídas: corredor oeste aberto e corredor leste com barricada fixa
- [ ] **2 · Sala de aula** — bilhete do aluno + chave da secretaria, carteiras viradas e desenhos nas paredes
- [ ] **3 · Secretaria** — planta da escola na parede (único mapa de interior do jogo), lista de chamada + chave do refeitório
- [ ] **4 · Refeitório** — barricada de mesas removível com E: **3 segundos e barulho alto**
- [ ] **5 · Corredor leste** — armário trancado à vista desde a entrada
- [ ] **6 · Sala dos professores** — chave do armário + última anotação
- [ ] **7 · Armário → FATIA Nº 6**, e a porta barricada do saguão se solta por dentro virando atalho de saída
- [ ] Nenhuma chave abre mais de uma porta (3 chaves, 3 barricadas)
- [ ] O Lobo não entra, mas **espera na porta** — sem indicação de qual lado
- [ ] Escola percorrida **só com o lampião** como fonte de luz

## 1.10 NPC da janela, falas e legendas (GDD §03)

- [ ] Sistema de **legenda narrativa** (diferente do prompt de interação já existente)
- [ ] Fila de falas com tempo de leitura, som de caractere e corte na pausa
- [ ] **Linha A · 3ª fatia**: disparada quando as luzes se apagam, indicando a fatia nº 8
- [ ] **Linha B · 6ª fatia**: só existe se a fatia nº 8 ainda não foi pega; substitui a linha A
- [ ] **Grito** no instante em que a fatia nº 8 é coletada
- [ ] Ao voltar: luz apagada, janela quebrada, casa arruinada — o NPC não fala mais
- [ ] Janela acesa como único "corpo" do NPC (referência Bloodborne), nunca renderizado
- [ ] Falas dos moradores na entrada de cada casa (6 falas únicas)

## 1.11 HUD, menus e opções (GDD §04)

- [x] **HUD diegética do braço**: sobe com a pausa, desce ao voltar (`ArmHUD`)
- [x] Relógio, pulseira de corações e baterias amarradas, redesenhando por evento
- [x] **ESC** abre/fecha o menu, braço na direita e botões na esquerda (`PauseMenu`)
- [x] Botões de pausa: Continue, Options, Main Menu, Quit, ligados por `OnClick`
- [x] `UIFadeIn` — botões entram em fade e não aceitam clique enquanto aparecem
- [x] `InteractPrompt` — "Pegar fatia", "Entrar", "Está trancado", com faixa escura de leitura
- [x] `InteractPrompt` escuta `ItemRequirement.OnAnyDenied` sozinho (recusa vira recado na tela)
- [x] Sons de interface: hover, clique, pause in/out, press start (`UISounds`)
- [x] Cena de **Main Menu** separada
- [ ] **Botões do menu principal** (hoje a primeira tela é só arte)
- [ ] Painel de **Options** com conteúdo: volume por camada (ambiente / Lobo / UI), brilho e idioma
- [ ] Tela/estado de **derrota** ("a noite te pegou") ligada no `NightRunner.onDeath`
- [ ] **Créditos** ao fim da cutscene
- [ ] Tela de controles / tutorial silencioso do Ato I

## 1.12 Cutscene final, morte e reinício (GDD §02)

- [x] `EndingSequence` com palco, ator único e troca de sprite Lobo → caçador
- [x] Input travado durante toda a sequência
- [x] Corte para preto, som de arma sendo carregada e tiro
- [x] Disparo pela porta da casa da avó via `Teleporter.onTeleported`
- [ ] **Beat 1**: Chapéuzinho entra e monta a torta sobre a mesa com as 8 fatias
- [ ] **Beat 2**: a "avó" se levanta, a cesta cai, corte para o quarto — restos, sangue e a touca da avó no chão
- [ ] **Beat 3**: o Lobo avança, se contorce e se destransforma no caçador
- [ ] **Beat 4**: ele olha as próprias mãos e encontra o papel "não sou eu" · corte para preto · tiro
- [ ] **Beat 5**: tela preta, relógio marcando **06:00 travado**, créditos
- [ ] Casa da avó **selada até 8/8** — a única transição irreversível do jogo

## 1.13 Localização (GDD §01)

- [ ] Sistema de idioma **PT-BR / Inglês** (legendas)
- [ ] Tabela de strings externa (prompts, falas, textos, menu, créditos)
- [ ] Seletor de idioma no menu de opções
- [ ] Revisão da tradução dos 7 textos e das 8 falas

## 1.14 Áudio — sistemas (GDD §11)

- [x] `MusicDirector` — faixa de fundo e faixa de tensão, **nunca simultâneas**, com crossfade
- [x] Silêncio real entre faixas (música é evento, não fundo permanente)
- [x] Troca automática para tensão quando o Lobo fareja ou persegue
- [x] `PlayerFootsteps` com clipes sorteados, ritmo e volume reduzido no agachado
- [x] `EnemyAudio` com escala por distância
- [x] Canal global de ruído `Noise` conectando som e IA
- [ ] **Mixagem separada por camada**: ambiente · Lobo · UI (AudioMixer com grupos e volume exposto)
- [ ] Ducking da trilha durante falas e sustos
- [ ] Ajuste de áudio na fase de otimização (GDD §11)

---

# 2 · Mapa e level design (GDD §06)

- [ ] 🟡 **Rua principal retilínea de ladrilho**, norte–sul, cortando o mapa *(tilesets importados, cidade não montada)*
- [ ] Trilhas de pedregulho ligando cada fachada (estilo Saint Denis)
- [ ] Todo o resto do solo em **grama**, igual ao da floresta — sem calçada, apagando a fronteira vila/mata
- [ ] **A–G**: os sete locais da planta posicionados conforme a arte do GDD
- [ ] **6 casas entráveis** com interior próprio
- [ ] **Casa do NPC** (não entrável) na trilha leste
- [ ] **Praça do poço** com o poço no centro
- [ ] **Labirinto de sebes** ao redor do poço, com sebes altas bloqueando linha de visão
- [ ] Corredor do labirinto **mais largo que o círculo do lampião**
- [ ] Dois becos sem saída do labirinto terminando em sangue
- [ ] 🟡 **Escola** — o maior interior do jogo, 6 ambientes *(objeto `EscolaInterior` já existe na cena)*
- [ ] 🟡 **Cabana do caçador** — casa pequena de pescador, a mais distante *(objeto `CabanaInterior` já existe na cena)*
- [ ] Cabana com rasgos nas paredes, carne humana e sangue **desde o início da partida**
- [ ] Gaiola de ferro no meio da sala da cabana + marreta encostada na parede
- [ ] **Loja de conveniência** — luzes acesas a noite toda e completamente vazia
- [ ] 🟡 **Casa da avó** no fim da rua principal *(objeto `CasaVovó` já existe na cena)*
- [ ] **Floresta** ao redor, com a árvore alta a noroeste
- [ ] Posicionamento das **8 fatias** nos locais do GDD §09
- [ ] Distribuição das **10 baterias**: 4 nas casas · 1 na cabana · 1 na loja · 1 na escola · 1 no labirinto · 2 ao ar livre
- [ ] Postes de luz distribuídos pela rua, com um `StreetLamp` em cada
- [ ] Ponto de spawn com a carta da mãe e a fatia nº 1
- [ ] Colisão do mapa inteiro revisada (nenhum canto onde o Lobo trava ou ela escapa)

---

# 3 · Sprites (GDD §11 — lista de assets)

**Padrão:** pixel art **acima do 8-bit**, mais cores por sprite e mais detalhe por tile, densidade do mock de menu como referência. Escala por número inteiro, *nearest neighbor*, sem filtro.

## 3.1 Personagens

- [x] **Chapéuzinho de pé** — idle + corrida em 5 folhas direcionais
- [ ] **Chapéuzinho agachada** (idle e andando)
- [ ] Chapéuzinho carregando escada
- [ ] Chapéuzinho carregando marreta
- [x] **Lobo Mau** — idle, caminhada e ataque (forma normal)
- [x] **Lobo transformado** — idle, caminhada e ataque (forma de garras / 5ª fatia)
- [x] **Lobo vestido de vovó** (cutscene)
- [x] **Caçador** — idle (cutscene)
- [ ] Caçador em **forma humana** fora da cama (se a cutscene pedir um segundo conjunto de poses)
- [ ] **Lobo atordoado** (caído e se levantando)
- [ ] **10 moradores** — sprites reaproveitáveis com troca de paleta
- [x] **Vovó** (sprite de cena/cutscene)

## 3.2 Cenário e tilesets

- [ ] 🟡 **Tileset da cidade** — grama, ladrilho, pedregulho, fachadas *(há `tileBaseTileset` importado; falta o set completo do GDD)*
- [ ] 🟡 **Interiores das casas — estado íntegro** *(há `TopDownHouse_FloorsAndWalls` + `FurnitureState1`)*
- [ ] **Interiores das casas — estado destruído** (parede rasgada, móveis virados, poças e respingos)
- [ ] **Sebes do labirinto**
- [ ] **Poço da vila**
- [ ] **Escola completa**: saguão, corredores, sala de aula, secretaria, refeitório, sala dos professores, armário
- [ ] **Barricadas** (a fixa, a de mesas removível e a que se solta por dentro)
- [ ] 🟡 **Floresta e árvore** *(`arvore 1`, `arvore2` importadas)*
- [ ] **Cabana** do caçador (exterior e interior)
- [ ] **Loja de conveniência** (exterior e interior)
- [ ] **Poças e respingos de sangue** (ruas, paredes e teto)
- [ ] Janela acesa do NPC (acesa, quebrada e apagada)
- [ ] 🟡 **Fachadas das casas** — `house1`, `house2`, `house3` importadas; faltam as 7 casas distintas do GDD
- [x] **Postes de luz** (`poste1`) e luminária
- [x] **Armário** (peça de cenário)

## 3.3 Itens e props

- [x] **Fatia de torta**
- [x] **Moeda**
- [x] **Chave** (base dourada — replicar para chave da cabana, da gaiola e as 3 da escola)
- [x] **Papéis / bilhetes** (`folha`, `folha caçador`)
- [x] **Máquina com teclado** (máquina de snacks)
- [ ] **Lampião elétrico** (na mão, aceso e apagado)
- [ ] **Bateria** (item no chão — o prefab existe, a arte não)
- [ ] **Escada** (parada, carregada e encostada na árvore)
- [ ] **Marreta** (inteira e quebrada)
- [ ] **Gaiola de ferro** (fechada e aberta)
- [ ] Rádio ligado (casa sul)
- [ ] Cesta da Chapéuzinho (cutscene)
- [ ] Touca da avó no chão (cutscene)
- [ ] 🟡 **Galho** *(item da solução atual da árvore; ver §9 Divergências)*

## 3.4 UI e ícones

- [x] Arte do **menu principal** + lobo do menu
- [x] Botão base da interface
- [x] **Braço da HUD**, relógio, pulseira e baterias
- [x] Sprite do relógio das 22:00 (quadro separado, `clock22`)
- [x] Fontes pixel (Pixeloid Mono / Sans / Sans Bold) com SDF gerado
- [ ] Folha completa do **relógio 23:00 → 06:00** (8 quadros)
- [ ] Folha da **pulseira** 3 → 1 coração (3 quadros)
- [ ] Folha das **baterias** 4 → 0 reservas (5 quadros) — revisar para as 10 baterias do GDD
- [ ] Ícone/arte da tela de derrota
- [ ] Arte dos créditos
- [ ] Painel de leitura de papéis e bilhetes

---

# 4 · Animações (GDD §11 — lista de assets)

- [x] **Caminhada da Chapéuzinho** (4 direções + diagonais, em Blend Tree 2D Simple Directional)
- [x] Idle da Chapéuzinho
- [ ] **Agachar e levantar** (estados `CrouchIdle` e `CrouchWalk` já esperados pelo `PlayerAnimatorBridge`)
- [ ] **Carregar escada**
- [ ] **Encostar escada e subir**
- [ ] **Carregar marreta**
- [ ] **Giro da marreta**
- [ ] **Marreta quebrando**
- [ ] **Troca de lampião por marreta**
- [x] **Corrida do Lobo** (walk) — formas normal e transformada
- [x] Idle e **ataque** do Lobo — formas normal e transformada
- [ ] **Farejar** do Lobo (4ª fatia)
- [ ] **Recuo** do Lobo (fuga da luz)
- [ ] **Bote** do Lobo (janela de ataque telegrafada)
- [ ] **Lobo atordoado e se levantando**
- [ ] **Chave caindo do corpo do Lobo**
- [ ] **Gaiola abrindo**
- [ ] **Queda da fatia na máquina**
- [ ] 🟡 **Postes piscando** *(feito por `Light2D` no `StreetLamp`; falta a animação de sprite, se houver)*
- [ ] **Sebes ao vento**
- [ ] **Troca das casas para o estado destruído**
- [ ] **Cutscene final completa** (os 5 beats de §1.12)
- [ ] Transição do Lobo → caçador (destransformação)
- [ ] Animação de dano e de morte da Chapéuzinho

---

# 5 · Sons — SFX (GDD §11 — lista de assets)

**Especificação:** composição em **FL Studio**, exportação em **WAV 16-bit / 44.1 kHz**.

## 5.1 Da jogadora e do mundo

- [x] **Passos** (`step01`, `step02`)
- [x] Passos na grama (`Grass`)
- [ ] **Abrir porta**
- [ ] **Agachar / levantar**
- [ ] **Arrastar e encostar escada**
- [ ] **Remover barricada** (3 s, alto)
- [ ] Som de dano na Chapéuzinho
- [ ] **Som de morte**
- [ ] **Ambiente geral** (vento, floresta, cidade vazia)
- [x] Piscada do poste (`Pole_Blink`)
- [ ] **Rádio com chiado** (casa sul)

## 5.2 Itens

- [ ] 🟡 **Pegar item** genérico (`Item_Pick`) — cobre provisoriamente fatia, chave, moeda e bateria
- [ ] **Pegar fatia** (som próprio — é a recompensa do jogo)
- [ ] **Pegar chave**
- [ ] **Pegar moeda**
- [ ] **Pegar bateria**
- [ ] **Trocar bateria**
- [x] **Ligar / desligar lampião** (`Lantern ON-OFF`)
- [ ] **Pegar marreta**
- [ ] **Giro da marreta no vazio**
- [ ] **Marretada acertando**
- [ ] **Marreta quebrando**
- [ ] **Chave caindo no chão**
- [ ] **Gaiola destrancando**
- [ ] **Queda da torta** (na máquina)
- [ ] **Teclado da máquina**
- [ ] **Erro da máquina (bipe alto)** — o que chama o Lobo

## 5.3 Do Lobo

- [x] **Uivo** (`uivo1`, `uivo2`)
- [x] **Bote / ataque** (`ataque1`)
- [ ] 🟡 **Lobo se aproximando** (`grunhido1`, `tensão1`) — falta a escada de respiração e passos por distância
- [x] Impacto / hit (`hit1`)
- [x] Lamento (`lamento1`)
- [x] Transformação (`transformationWolf`)
- [ ] **Lobo se afastando pela luz**
- [ ] **Lobo atordoado** (respiração arrastada, 15 s)
- [ ] Passos pesados do Lobo em camada própria, escalando por distância

## 5.4 Vozes, narrativa e interface

- [ ] **Falas dos moradores** (murmúrio — sem dublagem, por GDD)
- [ ] **Caracteres da legenda** (tique de texto)
- [ ] **Grito** do NPC da janela
- [x] **Tiro** (`gun_shooting`) e carregar arma (`load_gun`)
- [x] Hover de menu (`Mouse_Hover`)
- [x] Press start (`Press_Start`)
- [x] Pausa entrando e saindo (`Pause_IN`, `Pause_OUT`)
- [ ] Clique de botão confirmado / negado
- [ ] Som de recusa das trancas (`lockedClip` do `ItemRequirement`)

---

# 6 · Música e trilha (GDD §11)

- [ ] 🟡 **Trilha de fundo** — 1 faixa (`backgroundmusic1`); o `MusicDirector` aceita várias e sorteia sem repetir
- [ ] 🟡 **Trilha de tensão** — 1 faixa (`tensão1`)
- [ ] Faixas de fundo adicionais para a noite não ficar repetitiva (mínimo 3)
- [ ] Faixas de tensão adicionais (mínimo 2)
- [ ] Tema do **menu principal**
- [ ] Tema / stinger da **cidade de sangue** (6ª fatia)
- [ ] Stinger da **inversão da luz** (7ª fatia)
- [ ] Tema da **cutscene final** e dos **créditos**
- [ ] Stinger de morte / game over
- [ ] Camada de ambiente contínua, separada da trilha
- [ ] Todas as faixas em WAV 16-bit / 44.1 kHz e roteadas para os grupos do mixer

---

# 7 · Textos e falas (GDD §08)

## 7.1 Os textos legíveis

- [ ] **Carta da mãe** — ponto de spawn: objetivo + aviso de que o Lobo evolui a cada hora
- [ ] **Bilhete do vizinho** — casa nordeste: o homem da cabana passa pela vila de madrugada há semanas
- [ ] **Bilhete da professora** — casa oeste: por que a escola foi barricada por dentro
- [ ] **Bilhete do aluno** — sala de aula: "o homem da cabana veio de novo perguntar quantos alunos tinham faltado"
- [ ] **Lista de chamada** — secretaria: nomes riscados e a data da última aula, três dias atrás
- [ ] **Última anotação** — sala dos professores: "trancamos por dentro porque não adianta trancar por fora. NÃO SOU EU."
- [ ] **Papel da cabana** — "não sou eu" repetido até o fim da folha
- [ ] **Folha de estoque** — loja: o código do produto da torta (único texto com função mecânica)
- [ ] Consistência da **mesma letra** em três textos: bilhete da professora → última anotação → papel da cabana

## 7.2 As falas únicas dos moradores (uma por casa)

- [ ] **Noroeste** (3 moradores): "Fecha a porta. Ele já passou aqui uma vez hoje."
- [ ] **Nordeste** (homem velho): "Não ouço mais nada desde as dez. Isso é bom ou é ruim?"
- [ ] **Oeste** (professora e marido): "A escola está fechada. Barricamos por dentro. Não foi por causa de bicho."
- [ ] **Sudeste** (pescador): "Leva. Eu não vou mais subir em nada."
- [ ] **Sudoeste** (casal idoso): "A gente já contou as horas. Falta pouco."
- [ ] **Sul** (rapaz do rádio): "Meu irmão saiu pra ver o que era o barulho. Já faz tempo."

## 7.3 NPC da janela acesa

- [ ] **Linha A** (3ª fatia): "Tem uma coisa tua caída lá no mato, do lado sul. Não vai com a luz acesa."
- [ ] **Linha B** (6ª fatia, só se a fatia nº 8 ainda não foi pega): "Não bate mais nas outras portas. Não tem mais ninguém atrás delas."

## 7.4 Textos de sistema

- [x] Prompts de interação ("Pegar fatia", "Entrar", "Está trancado")
- [ ] Recados de tranca de cada puzzle ("Não alcanço", "Falta a chave", "Precisa de moeda")
- [ ] Textos de menu, opções e créditos
- [ ] Versão em inglês de tudo acima

---

# 8 · Técnico, build e otimização (GDD §11)

## 8.1 Setup de projeto

- [x] Unity **2022.3.62f3 (LTS)** com pipeline **2D URP**
- [x] Estrutura `Assets/_Project/{Art, Audio, Prefabs, Scenes, Scripts, UI}`
- [x] Versionamento Git com commits por sistema
- [x] `Sprite-Lit-Default` nos sprites que reagem à luz
- [x] `Transparency Sort Mode` = Custom Axis `(0,1,0)` — Y-sorting
- [x] Canvas Scaler em `160 × 144`, Match Height
- [ ] **Pixel Perfect Camera** configurada
- [ ] **Pixels per unit fixo** em todo o projeto (auditar imports)
- [ ] Resolução interna escalada por inteiro, *nearest neighbor*, sem filtro nem suavização (auditar `Filter Mode` e compressão de todos os sprites)
- [ ] **AudioMixer** com os grupos ambiente / Lobo / UI
- [ ] Layer própria para interagíveis (o `interactableLayers` está em *Everything* e o buffer do `OverlapCircle` é 8)
- [ ] Limpeza de branches e merge na `main` (há 7 branches locais e 11 remotas abertas)

## 8.2 Balanceamento (GDD §05, §11)

- [ ] **10 baterias × 60 s** conferidas em partida de 15–30 min (se a noite ficar clara demais, cortar para 30 s por bateria **antes** de mexer na quantidade)
- [ ] Largura dos corredores do labirinto contra o raio do lampião
- [ ] Velocidades, raios de percepção e de visão do Lobo por estágio
- [ ] Janela do giro da marreta (dificuldade do acerto no escuro)
- [ ] Tempo de invulnerabilidade e knockback
- [ ] Ritmo do relógio × tempo real de travessia do mapa

## 8.3 Qualidade e entrega

- [ ] Playtest completo de uma partida inteira (22:00 → 06:00)
- [ ] Playtest de ordens alternativas de coleta (checando deadlock)
- [ ] Passagem de performance (luzes 2D, colliders, tilemaps)
- [ ] Build **Windows** rodando fora do editor
- [ ] Ícone e nome do executável
- [ ] Instruções de instalação e controles para a página do jogo
- [ ] Vídeo / GIF de divulgação e capturas de tela

---

# 9 · Divergências entre o GDD v1.5 e o projeto (decidir antes de fechar)

| # | GDD v1.5 diz | O projeto tem hoje | Ação |
|---|---|---|---|
| 1 | Fatia nº 4 é resolvida com a **escada** da casa sudeste, carregada no lugar do lampião | Um item **`galho`** com `KnockDown` derruba a fatia da árvore | Escolher: implementar a escada ou atualizar o GDD |
| 2 | Fatia nº 7 exige **moeda + código** digitado no teclado da máquina | Moeda e máquina existem; sem teclado, sem código e sem bipe de erro | Implementar o teclado e ligar a folha de estoque |
| 3 | HUD do §04 com relógio, corações e bateria **nos cantos da tela** | HUD **diegética no braço**, que sobe com a pausa | Substituição consciente — registrar no GDD |
| 4 | **10 baterias** no mapa | `ArmHUD` desenha **4 slots** de reserva | Ajustar a folha da bateria ou a leitura do HUD |
| 5 | Rastreamento por **trilha de sangue** na 4ª fatia | Representado só por `senseRadius` aberto | Implementar a trilha ou assumir a simplificação |
| 6 | Marreta pode ser pega a qualquer momento | Sem guarda nenhuma | Bloquear a coleta enquanto o Lobo não estiver no mapa (o próprio GDD recomenda) |

---

# 10 · Fora de escopo (GDD §11 — não implementar)

- Múltiplos finais
- Sistema de save
- Combate (o único revide do jogo é a marretada, que só atordoa)
- Inventário além dos slots fixos
- Dublagem

---

# 11 · Cronograma (GDD §11)

| Fase | Dias | Entrega |
|---|---|---|
| Escrever história | 1 | Roteiro fechado, cutscene beat a beat, falas e bilhetes |
| Montar GDD | 1 | Escopo travado antes de qualquer linha de código |
| Prototipagem, som e assets | 7 | Movimentação, agachar, lampião, IA do Lobo, sprites, animações e trilha |
| Criação do game | 5 | Cidade, interiores, escola, labirinto, 8 fatias, 6 puzzles, marreta, menu, cutscene, progressão |
| Otimização | 3 | Balanceamento, ajuste de áudio, build e polimento |
| **Total** | **17** | Pré-produção 2 · Produção 12 · Polimento 3 |

**Ordem recomendada pelo GDD:** prototipar primeiro o par **lampião + IA do Lobo** numa cena vazia — se essa relação não gerar tensão sozinha, nenhum puzzle ou sprite salva o jogo. Depois, a **troca de estado das casas**: o efeito mais barato de implementar e o que mais muda a partida inteira.

> ⚠️ **Alerta de escopo do GDD:** sair do 8-bit para a densidade do mock de menu multiplica o custo de arte de todo o projeto. Se algo tiver que ceder, ceda primeiro no **número de interiores únicos**, não no padrão visual. O primeiro candidato a encolher é a **escola** — cortando a sala dos professores e movendo a chave do armário para o refeitório.
