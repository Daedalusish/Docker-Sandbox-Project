


1/ install guidde
Windows: 
Enable-WindowsOptionalFeature -Online -FeatureName HypervisorPlatform -All

MAC os:
Apple silicon

Linux
¯\_(ツ)_/¯ experience may vary
(Demo  i Bazzite, fedora fork. Kommer med KVM prekonfigurert men ingen apt støtte). Offisielt kun støttet på Ubuntu. Magic of linux lar det kjøre men your experience may wary

Docker optional (Jeg kjører podman med alias docker=podman, don't worry  about it). Docker sandbox krever en "Engine" som kan erstattes med engine of choice.  



Phase 2: Sign in
(sbx login, requires docker account regardless of engine) 
sbx diagnose to check stuff running
sbx daemon start

sbx tui -> Ingen GUI men TUI.  

Scenario A: 
Basic sandbox runninng claude
1)Imma be real: Bare bruk web løsningen er sikrere og bedre enn dette men proof of concept her. 
