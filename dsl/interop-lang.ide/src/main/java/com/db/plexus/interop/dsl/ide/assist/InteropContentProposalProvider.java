/**
 * Copyright 2017-2018 Plexus Interop Deutsche Bank AG
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.db.plexus.interop.dsl.ide.assist;

import com.db.plexus.interop.dsl.ConsumedService;
import com.db.plexus.interop.dsl.ProvidedService;
import com.db.plexus.interop.dsl.protobuf.Method;
import com.db.plexus.interop.dsl.protobuf.ProtoLangImportResolver;
import com.db.plexus.interop.dsl.protobuf.Service;
import com.google.common.base.Predicate;
import com.google.inject.Inject;
import org.eclipse.emf.ecore.EObject;
import org.eclipse.xtext.*;
import org.eclipse.xtext.ide.editor.contentassist.ContentAssistContext;
import org.eclipse.xtext.ide.editor.contentassist.ContentAssistEntry;
import org.eclipse.xtext.ide.editor.contentassist.IIdeContentProposalAcceptor;
import org.eclipse.xtext.ide.editor.contentassist.IdeContentProposalProvider;
import org.eclipse.xtext.impl.RuleCallImpl;
import org.eclipse.xtext.resource.IEObjectDescription;

public class InteropContentProposalProvider extends IdeContentProposalProvider {

    @Inject
    private ProtoLangImportResolver importResolver;

    @Override
    protected void _createProposals(Assignment assignment, ContentAssistContext context, IIdeContentProposalAcceptor acceptor) {
        final AbstractElement terminal = assignment.getTerminal();
        if (terminal instanceof RuleCallImpl) {
            final String feature = assignment.getFeature();
            final String operator = assignment.getOperator();
            if ("importURI".equals(feature) && "=".equals(operator)) {
                importResolver.importCandidates(context.getResource())
                        .stream()
                        .map(uri -> uri.toFileString().replace("\\", "/"))
                        .map(uriString -> "\"" + uriString + "\"")
                        .forEach(value -> {
                            final ContentAssistEntry proposal = getProposalCreator()
                                    .createProposal(value, context);
                            acceptor.accept(proposal, getProposalPriorities().getDefaultPriority(proposal));
                        });
                return;
            }
        }
        super._createProposals(assignment, context, acceptor);
    }

    protected Predicate<IEObjectDescription> getCrossrefFilter(final CrossReference reference, final ContentAssistContext context) {

        return eObjectProposal -> {

            final EObject contextCurrentModel = context.getCurrentModel();
            final EObject proposed = eObjectProposal.getEObjectOrProxy();

            if (proposed instanceof Method && contextCurrentModel instanceof ConsumedService) {
                final ConsumedService consumedService = (ConsumedService) contextCurrentModel;
                final Method method = (Method) proposed;
                final Service methodService = EcoreUtil2.getContainerOfType(method, Service.class);
                return consumedService.getService().equals(methodService);
            }

            if (proposed instanceof Method && contextCurrentModel instanceof ProvidedService) {
                final ProvidedService providedService = (ProvidedService) contextCurrentModel;
                final Method method = (Method) proposed;
                final Service methodService = EcoreUtil2.getContainerOfType(method, Service.class);
                return providedService.getService().equals(methodService);
            }

            return true;
            
        };
    }

}
