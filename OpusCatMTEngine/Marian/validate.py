import sacrebleu
import sys

#Both input files are expected to be segmented (either SentencePiece or BPE).

valid_target_path = sys.argv[1]
ood_size = int(sys.argv[2])
system_seg_method = sys.argv[3]
system_output_path = sys.argv[4]

def extract_lines_and_split(sent_file_path, seg_method=None):
	with open(sent_file_path,'rt', encoding='utf-8') as sent_file:
		sents = sent_file.readlines()

	if seg_method is not None:
		if seg_method == ".bpe":
			sents = [x.replace("@ ","") for x in sents]
		elif seg_method == ".spm":
			sents = [x.replace(" ","").replace("▁"," ") for x in sents]

	ood_sents = sents[0:ood_size]
	with open(sent_file_path +"_0.txt", 'wt', encoding='utf-8') as ood_file:
		for ood_sent in ood_sents:
			ood_file.write(ood_sent);

	indomain_sents = sents[ood_size:]
	with open(sent_file_path +"_1.txt", 'wt', encoding='utf-8') as indomain_file:
		for indomain_sent in indomain_sents:
			indomain_file.write(indomain_sent);

	return (ood_sents,indomain_sents)

valid_ood_sents, valid_indomain_sents = extract_lines_and_split(valid_target_path,system_seg_method)
system_ood_sents, system_indomain_sents = extract_lines_and_split(system_output_path,system_seg_method)

ood_bleu = sacrebleu.corpus_bleu(system_ood_sents,[valid_ood_sents])
ood_chrf = sacrebleu.corpus_chrf(system_ood_sents,[valid_ood_sents])
with open(system_output_path +"_0.score.txt", 'wt', encoding='utf-8') as ood_score_file:
	ood_score_file.write("BLEU: %.2f"%(ood_bleu.score)+"\n")
	ood_score_file.write("chrF: %.2f"%(ood_chrf.score))

indomain_bleu = sacrebleu.corpus_bleu(system_indomain_sents,[valid_indomain_sents])
indomain_chrf = sacrebleu.corpus_chrf(system_indomain_sents,[valid_indomain_sents])
with open(system_output_path +"_1.score.txt", 'wt', encoding='utf-8') as indomain_score_file:
	indomain_score_file.write("BLEU: %.2f"%(indomain_bleu.score)+"\n")	
	indomain_score_file.write("chrF: %.2f"%(indomain_chrf.score))

sys.stdout.write("%.2f"%((ood_bleu.score+indomain_bleu.score)))
