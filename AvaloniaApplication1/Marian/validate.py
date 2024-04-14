import sacrebleu
import sys
import argparse

#This is used as the valid script in a Marian training config
#Both input files are expected to be segmented (either SentencePiece or BPE).

parser = argparse.ArgumentParser(
    prog='validator',
    description='Validates Marian output for OPUS-CAT, splits the valid set into ood and in-domain')


parser.add_argument('--ood_size', type=int,
                    help='size of the ood set')
parser.add_argument('--seg_method', type=str,
                    help='spm or sentencepiece')
# Nargs is used as a cross-platform workaround for spaces in paths
parser.add_argument('--valid_target', type=str, nargs='+',
                    help='path to target ref')
parser.add_argument('--system_output', type=str, nargs='+',
                    help='path to system output')


args = parser.parse_args()

valid_target_path = " ".join(args.valid_target)
ood_size = args.ood_size
system_seg_method = args.seg_method
system_output_path = " ".join(args.system_output)

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
