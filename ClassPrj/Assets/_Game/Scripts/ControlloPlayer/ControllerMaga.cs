﻿using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CapsuleCollider)), RequireComponent(typeof(Animator)), RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(EventoAudio))]
public class ControllerMaga : MonoBehaviour
{
    #region Variabili PUBLIC

    public bool corsaPerDefault = false;

    [Range(0.3f, 1f)]
    public float distanzaDaTerra = 0.44f;

    public float distanzaMassimaClick = 20f;

    [Range(4f, 10f)]
    public float forzaSalto = 4f;

    //public bool IsPointAndClick;

    #endregion Variabili PUBLIC

    #region Variabili PRIVATE

    private const float meta = 0.5f;
    private bool abbassato;
    private float altezzaCapsula;
    private Animator animatore;
    private bool aTerra;
    private AudioZona audioZona;
    private CapsuleCollider capsula;
    private Vector3 capsulaCentro;
    private float cicloOffset = 0.2f;
    private DatiPersonaggio datiPersonaggio;
    private bool Destinazione = false;
    private EventoAudio ev_Audio;
    private GestoreCanvasNetwork gestoreCanvas;
    private float h, v;
    private float rotazione;
    private RaycastHit hit;
    private float jumpLeg;
    private LayerMask layer = 1 << 13;
    private LayerMask layerAlberi = 1 << 13;
    private ManagerNetwork managerNetwork;
    private Vector3 movimento;
    private NavMeshAgent navMeshAgent;
    private Vector3 posMouse;
    private Rigidbody rigidBody;
    private bool rimaniBasso;
    private SwitchVivoMorto switchVivoMorto;

    private float forward;
    private bool attacco1 = false;
    private bool attacco2 = false;
    private float jump;


    private Transform transform_m;
    private float velocitaSpostamento;
    private bool voglioSaltare = false;
    private bool rotBool=false;  //serve per il multigiocatore, quando uso modalita tastiera..(gli fa passare true se abilito la rotazione )

    //Classe Network
    private NetworkPlayer net;
    //

    public DatiPersonaggio DatiPersonaggio
    {
        get
        {
            return datiPersonaggio;
        }

        set
        {
            datiPersonaggio = value;
        }
    }

    public NavMeshAgent NavMeshAgent
    {
        get
        {
            return navMeshAgent;
        }
    }

    public Rigidbody RigidBody
    {
        get
        {
            return rigidBody;
        }
    }

    public NetworkPlayer Net
    {
        get
        {
            return net;
        }

        set
        {
            net = value;
        }
    }

    public float Forward
    {
        get
        {
            return forward;
        }


    }

    public bool Attacco1
    {
        get
        {
            return attacco1;
        }


    }

    public bool Attacco2
    {
        get
        {
            return attacco2;
        }


    }

    public float Rotazione
    {
        get
        {
            return rotazione;
        }

    }

    public bool ATerra
    {
        get
        {
            return aTerra;
        }

    }

    public float JumpLeg
    {
        get
        {
            return jumpLeg;
        }

    }

    public float Jump
    {
        get
        {
            return jump;
        }
    }

    public bool RotBool
    {
        get
        {
            return rotBool;
        }

    }


    #endregion Variabili PRIVATE

    //richiamare questo metodo come evento dell'animazione attacco nel frame finale o se viene lanciaato qualcosa appena viene colpito il bersaglio
    //quindi a seconda dell'attacco.
    public void FaiDanno()
    {
        //controllare se il personaggio è girato verso il nemico e quindi se è nel suo arco di attacco
        //farsi dare la percentuale di resistere all'attacco dal nemico
        //calcolare un numero random da 1 a 100, per esempio supponendo che la percentuale del nemico di resistere sia del 20%,
        //se il  numero random è un numero inferiore a 20 l'attacco non è andato a buon fine se invece è un numero da 21 a 100 è andato a buonfine.
        //se l'attacco è andato a buonfine:
        //recuperare attaccobase del personaggio da DatiPersonaggio, e recuperare tutti i dati del nemico relativi alla sua difesa
        //calcolare il danno da effettuare in base a tutti i valori sopra citati(secondo una qualche equazione che li lega)
        //mandare un messaggio al metodo del nemico RiceviDanno passando come parametro la quantità di danno inflitta
    }

    public void PozioneVita(float quanto)
    {
        if (DatiPersonaggio.Vita > 0f)
        {
            DatiPersonaggio.Vita += quanto;
            SalvaDatiVita();
        }
    }

    public void Resuscita(float quanto)
    {
        DatiPersonaggio.Vita += quanto;
        switchVivoMorto.DisattivaRagdoll();
        SalvaDatiVita();
        Statici.PersonaggioPrincipaleT.position = GameObject.Find(Statici.datiPersonaggio.Dati.posizioneCheckPoint).transform.position;
    }

    public void RiceviDanno(float quanto)
    {
        DatiPersonaggio.Vita -= quanto;
        if (DatiPersonaggio.Vita <= 0f)
            switchVivoMorto.AttivaRagdoll();
        SalvaDatiVita();
    }

    private void Attacco()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && !animatore.GetCurrentAnimatorStateInfo(0).IsName("Attacco1") && !animatore.GetCurrentAnimatorStateInfo(0).IsName("Attacco2"))
        {
            if (Input.GetMouseButtonDown(0) && !voglioSaltare && aTerra)
            {
                animatore.SetTrigger("attacco1");
                attacco1 = true;
            }

            if (Input.GetMouseButtonDown(1) && !voglioSaltare && aTerra)
            {
                animatore.SetTrigger("attacco2");
                attacco2 = true;
            }
        }
    }

    private void FixedUpdate()
    {
      
        if ((Statici.multigiocatoreOn && !DatiPersonaggio.SonoUtenteLocale))   //AGGIUNTO DA LUCA
            { 
            return;
        }
        
        aTerra = false;
        RaycastHit hit;

        //  Debug.DrawLine(transform_m.position + (Vector3.up * 0.1f), transform_m.position + (Vector3.down * 0.1f), Color.blue);
        if (Physics.Raycast(transform_m.position + (Vector3.up * 0.1f), Vector3.down, out hit, distanzaDaTerra))
        {
            aTerra = true;
            animatore.applyRootMotion = !Statici.IsPointAndClick;
        }
        else
        {
            aTerra = false;
            animatore.applyRootMotion = false;
            voglioSaltare = false;
        }
        ///*SALTO
        if (voglioSaltare && aTerra)
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, forzaSalto, rigidBody.velocity.z);
            float cicloCamminata = Mathf.Repeat(animatore.GetCurrentAnimatorStateInfo(0).normalizedTime + cicloOffset, 1);
            jumpLeg = (cicloCamminata < 0.5f ? 1 : -1) * movimento.z;
        }

        /*CONTROLLO ABBASSATO
        Ray ray = new Ray(transform_m.position + (Vector3.up * capsulaCentro.y), Vector3.up);

        float lunghezzaRay = capsulaCentro.y;

        if (Physics.SphereCast(ray, capsula.radius * meta, lunghezzaRay) && abbassato)
            rimaniBasso = true;
        else
            rimaniBasso = false;
         */
        /* ANIMATORE */

        if (!Statici.IsPointAndClick)
        {
            animatore.SetFloat("Forward", movimento.z * velocitaSpostamento);
            forward = movimento.z * velocitaSpostamento;
        }

      //  Debug.Log("transform.localEulerAngles.y  " + transform.localEulerAngles.y);
        animatore.SetBool("OnGround", aTerra);
        animatore.SetFloat("Forward", forward);
        animatore.SetFloat("Turn", rotazione);  //server solo se si muove personaggio tramite tastiera
        animatore.SetFloat("JumpLeg", jumpLeg); //server solo se si muove personaggio tramite tastiera

      //  Debug.Log(" position" + transform.position + "rotazione " + transform.rotation + "Contro rotaz " + rotazione);

        //animatore.SetBool("Crouch", abbassato);
        if (!aTerra && !Statici.IsPointAndClick)
        {
            jump = rigidBody.velocity.y;
            animatore.SetFloat("Jump", jump);  //server solo se si muove personaggio tramite tastiera
        }

    }



    private void SalvaDatiVita()
    {
        Statici.datiPersonaggio.Dati.Vita = DatiPersonaggio.Vita;
        Statici.datiPersonaggio.Salva();
        GestoreCanvasAltreScene.AggiornaVita();
    }

    private void Start()
    {
        transform_m = GetComponent<Transform>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            navMeshAgent.height = 2f;
            navMeshAgent.stoppingDistance = 1f;
        }
        navMeshAgent.enabled = true;

        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        rigidBody.isKinematic = true;

        ev_Audio = GetComponent<EventoAudio>();
        if (ev_Audio == null)
            ev_Audio = gameObject.AddComponent<EventoAudio>();

        audioZona = GetComponent<AudioZona>();
        if (audioZona == null)
            audioZona = gameObject.AddComponent<AudioZona>();

        animatore = GetComponent<Animator>();
        capsula = GetComponent<CapsuleCollider>();
        altezzaCapsula = capsula.height;
        capsulaCentro = new Vector3(0.0f, capsula.center.y, 0.0f);
        //IsPointAndClick = true;
        layerAlberi = ~layerAlberi;
        switchVivoMorto = GetComponent<SwitchVivoMorto>();
        if (!Statici.inGioco)
            return;
        DatiPersonaggio = GetComponent<DatiPersonaggio>();

        if (!Statici.multigiocatoreOn)
        {
            Statici.RegistraDatiPersonaggio(DatiPersonaggio);
            //se all'inizio della partita si ritrova a 0 di vita, gli do 1 di vita così non nasce morto.
            if (DatiPersonaggio.Vita <= 0f)
            {
                DatiPersonaggio.Vita = 1f;
                SalvaDatiVita();
            }
        }
        else
        {
            managerNetwork = GameObject.Find("ManagerNetwork").GetComponent<ManagerNetwork>();
            gestoreCanvas = GameObject.Find("ManagerCanvasMultiplayer").GetComponent<GestoreCanvasNetwork>();
        }
    }

    private void Update()
    {
        attacco1 = false;
        attacco2 = false;
        //AGGIUNTA MULTIPLAYER
        if (!Statici.inGioco || (Statici.multigiocatoreOn && !DatiPersonaggio.SonoUtenteLocale))
        {
            rigidBody.isKinematic = false;
            capsula.enabled = true;
            navMeshAgent.enabled = false;
            return;
        }
        //
        if (Statici.IsPointAndClick)
        {
            if (navMeshAgent.enabled == false)
            {
                rigidBody.isKinematic = true;
                capsula.enabled = false;
                navMeshAgent.enabled = true;
            }
            if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !EventSystem.current.IsPointerOverGameObject()
                && !animatore.GetCurrentAnimatorStateInfo(0).IsName("Attacco1")
                && !animatore.GetCurrentAnimatorStateInfo(0).IsName("Attacco2"))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, distanzaMassimaClick, layerAlberi, QueryTriggerInteraction.Ignore))
                {
                    posMouse = hit.point;

                    switch (hit.transform.gameObject.layer) //Bozza da correggere ... Ho usato Tag Goblin per provare .
                    {
                        case 0:
                            if (Vector3.Distance(transform_m.position, posMouse) > navMeshAgent.stoppingDistance)
                                navMeshAgent.SetDestination(posMouse);
                            break;

                        case 11:
                            if (hit.collider.transform != gameObject.transform) //se non sono io
                                Attacco();
                            break;
                    }
                }
            }
            forward = navMeshAgent.velocity.normalized.magnitude;

            
            //       animatore.SetFloat("Forward", forward);   TOLTO DA LUCA...va anche sensa..e' una ripetizione.

        }
        else // Not Point & Click
        {
            if (rigidBody.isKinematic == true)
            {
                rigidBody.isKinematic = false;
                capsula.enabled = true;
                navMeshAgent.enabled = false;
            }

            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            rotBool = false;
            if (Input.GetButton("Horizontal")) rotBool = true;  //server multigiocatore...uso con tastiera..
           //Debug.Log("jump  " + jump); 
                movimento = new Vector3(h, 0.0f, v);
            rotazione = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
            velocitaSpostamento = !Input.GetKey(KeyCode.LeftShift) && corsaPerDefault ||
                                  !corsaPerDefault && Input.GetKey(KeyCode.LeftShift) ? 1f : 0.5f;

            if (Input.GetButtonDown("Jump") && aTerra && !voglioSaltare && !animatore.GetCurrentAnimatorStateInfo(0).IsName("Attacco1") &&
                !animatore.GetCurrentAnimatorStateInfo(0).IsName("Attacco2"))
                voglioSaltare = true;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                Attacco();

            /* ACCOVACCIAMENTO
            if (!voglioSaltare && aTerra && Input.GetKey(KeyCode.C))
             {
                 abbassato = true;
                 capsula.center = capsulaCentro * meta;
                 capsula.height = capsula.height * meta;
             }
             else
             {
                 if (!rimaniBasso)
                 {
                     abbassato = false;
                     capsula.height = altezzaCapsula;
                     capsula.center = capsulaCentro;
                     rimaniBasso = false;
                 }
             }
             */
        }

    }

    /*
      void OnMouseDown()
      {
          if (!Statici.multigiocatoreOn || (Statici.multigiocatoreOn && DatiPersonaggio.SonoUtenteLocale))
              return;
          managerNetwork.NemicoColpito(DatiPersonaggio.Utente);
          gestoreCanvas.ResettaScrittaNemicoAttaccato(true);
          gestoreCanvas.UserDaVisualizzareNemico = DatiPersonaggio.Utente;
          gestoreCanvas.VitaDaVisualizzareNemico = DatiPersonaggio.Vita.ToString();
      }
      void OnMouseExit()
      {
            if(Statici.inGioco)
              gestoreCanvas.ResettaScrittaNemicoAttaccato(false);
      }*/
}